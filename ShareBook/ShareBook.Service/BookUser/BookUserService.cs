using FluentValidation;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using ShareBook.Domain;
using ShareBook.Domain.Common;
using ShareBook.Domain.Enums;
using ShareBook.Domain.Exceptions;
using ShareBook.Repository;
using ShareBook.Repository.UoW;
using ShareBook.Service.Generic;
using ShareBook.Service.Muambator;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace ShareBook.Service
{
    public class BookUserService : BaseService<BookRequest>, IBookUserService
    {
        private readonly IBookRequestRepository _bookUserRepository;
        private readonly IBookService _bookService;
        private readonly IBookUsersEmailService _bookUsersEmailService;
        private readonly IMuambatorService _muambatorService;
        private readonly IBookRepository _bookRepository;
        private readonly IConfiguration _configuration;
        private readonly IUserService _userService;

        public BookUserService(
            IBookRequestRepository bookUserRepository, 
            IBookService bookService,
            IBookUsersEmailService bookUsersEmailService,
            IMuambatorService muambatorService,
            IBookRepository bookRepository,
            IUnitOfWork unitOfWork,
            IValidator<BookRequest> validator, IConfiguration configuration, IUserService userService)
            : base(bookUserRepository, unitOfWork, validator)
        {
            _bookUserRepository = bookUserRepository;
            _bookService = bookService;
            _bookUsersEmailService = bookUsersEmailService;
            _muambatorService = muambatorService;
            _bookRepository = bookRepository;
            _configuration = configuration;
            _userService = userService;
        }

        public IList<User> GetGranteeUsersByBookId(Guid bookId) =>
            _bookUserRepository.Get().Include(x => x.DonorUser)
            .Where(x => x.BookRequestedId == bookId && x.Status == DonationStatus.WaitingAction)
            .Select(x => x.DonorUser.Cleanup()).ToList();

        // TODO: avaliar se o uso de custom sql melhora significativamente a performance. Muitos includes.
        public IList<BookRequest> GetRequestersList(Guid bookId) =>
            _bookUserRepository.Get()
            .Include(x => x.DonorUser).ThenInclude(u => u.Address)
            .Include(x => x.DonorUser).ThenInclude(u => u.BookUsers)
            .Include(x => x.DonorUser).ThenInclude(u => u.BooksDonated)
            .Where(x => x.BookRequestedId == bookId)
            .OrderBy(x => x.CreationDate)
            .ToList();

        public void Insert(Guid bookId, string reason)
        {
            //obtem o livro requisitado e o doador
            var bookRequested = _bookService.GetBookWithAllUsers(bookId);
            var requestUserId = new Guid(Thread.CurrentPrincipal?.Identity?.Name);
            var requestUser = _userService.Find(requestUserId);

            var bookRequest = new BookRequest()
            {
                BookRequestedId = bookId,
                RequestUserId = requestUserId,
                RequestUser = requestUser,
                Reason = reason,
                NickName = $"Interessado {bookRequested?.TotalInterested() + 1}",
                DonorUser = bookRequested.User,
                BookRequested = bookRequested   
            };

            if (!_bookService.Any(x => x.Id == bookRequest.BookRequestedId))
                throw new ShareBookException(ShareBookException.Error.NotFound);

            if (_bookUserRepository.Any(x => x.RequestUserId == bookRequest.RequestUserId && x.BookRequestedId == bookRequest.BookRequestedId))
                throw new ShareBookException("O usuário já possui uma requisição para o mesmo livro.");

            if (bookRequested.Status != BookStatus.Available)
                throw new ShareBookException("Esse livro não está mais disponível para doação.");

            _bookUserRepository.Insert(bookRequest);

            // Remove da vitrine caso o número de pedidos estiver grande demais.
            MaxRequestsValidation(bookRequested);

            //_bookUsersEmailService.SendEmailBookRequested(bookUser).Wait();
            //_bookUsersEmailService.SendEmailBookDonor(bookUser, bookRequested).Wait();
            //_bookUsersEmailService.SendEmailBookInterested(bookUser, bookRequested).Wait();
        }

        private void MaxRequestsValidation(Book bookRequested)
        {
            var maxRequestsPerBook = int.Parse(_configuration["SharebookSettings:MaxRequestsPerBook"]);
            if (bookRequested.BookUsers.Count < maxRequestsPerBook)
                return;

            bookRequested.Status = BookStatus.AwaitingDonorDecision;
            bookRequested.ChooseDate = DateTime.Today.AddDays(1);
            _bookRepository.Update(bookRequested);

            _bookUsersEmailService.SendEmailMaxRequests(bookRequested).Wait();
        }

        public void DonateBook(Guid bookId, Guid userId, string note)
        {
            var book = _bookService.Find(bookId);
            if (!book.MayChooseWinner())
                throw new ShareBookException(ShareBookException.Error.BadRequest, "Aguarde a data de decisão.");

            var bookUserAccepted = _bookUserRepository.Get()
                .Include(u => u.BookRequested).ThenInclude(b => b.UserFacilitator)
                .Include(u => u.BookRequested).ThenInclude(b => b.User)
                .Include(u => u.DonorUser).ThenInclude(u => u.Address)
                .Where(x => x.RequestUserId == userId
                    && x.BookRequestedId == bookId
                    && x.Status == DonationStatus.WaitingAction)
                    .FirstOrDefault();

            if (bookUserAccepted == null)
                throw new ShareBookException("Não existe a relação de usuário e livro para a doação.");

            bookUserAccepted.UpdateBookRequest(DonationStatus.Donated, note);

            _bookUserRepository.Update(bookUserAccepted);

            DeniedBookUsers(bookId);

            _bookService.UpdateBookStatus(bookId, BookStatus.WaitingSend);

            // usamos await nas notificações porque eventualmente tem risco da taks
            // não completar o trabalho dela. Talvez tenha a ver com o garbage collector.

            // avisa o ganhador
            _bookUsersEmailService.SendEmailBookDonated(bookUserAccepted).Wait();

            // avisa os perdedores :/
            NotifyInterestedAboutBooksWinner(bookId).Wait();

            // avisa o doador
            _bookUsersEmailService.SendEmailBookDonatedNotifyDonor(bookUserAccepted.BookRequested, bookUserAccepted.DonorUser).Wait();
        }

        public Result<Book> Cancel(Guid bookId, bool isAdmin = false)
        {
            var book = _bookService.Find(bookId);

            if (book == null)
                throw new ShareBookException(ShareBookException.Error.NotFound);

            var bookUsers = _bookUserRepository.Get().Where(x => x.BookRequestedId == bookId).ToList();

            book.ChooseDate = null;
            book.Status = BookStatus.Canceled;

            CancelBookUsersAndSendNotification(book);            

            _bookService.Update(book);
            _bookUsersEmailService.SendEmailBookCanceledToAdmins(book).Wait();

            return new Result<Book>(book);
        }

        public void DeniedBookUsers(Guid bookId)
        {
            var bookUsersDenied = _bookUserRepository.Get().Where(x => x.BookRequestedId == bookId
            && x.Status == DonationStatus.WaitingAction).ToList();
            foreach (var item in bookUsersDenied)
            {
                string note = string.Empty;
                item.UpdateBookRequest(DonationStatus.Denied, note);
                _bookUserRepository.Update(item);
            }
        }

        private void CancelBookUsersAndSendNotification(Book book){
            DeniedBookUsers(book.Id);
            NotifyUsersBookCanceled(book);
        }

        public PagedList<BookRequest> GetRequestsByUser(int page, int itemsPerPage)
        {
            var userId = new Guid(Thread.CurrentPrincipal?.Identity?.Name);
            var query = _bookUserRepository.Get()
                .Include(x => x.BookRequested)
                .Where(x => x.RequestUserId == userId)
                .OrderByDescending(x => x.CreationDate);

            var result = FormatPagedList(query, page, itemsPerPage);

            // só mostra o código de rastreio se ele for o ganhador.
            result.Items = result.Items.Select(bu =>
            {
                bu.BookRequested.TrackingNumber = bu.Status == DonationStatus.Donated ? bu.BookRequested.TrackingNumber : null;
                return bu;
            }).ToList();

            return result;
        }

        public async Task NotifyInterestedAboutBooksWinner(Guid bookId)
        {
            //Obter todos os users do livro
            var bookUsers = _bookUserRepository.Get()
                                                .Include(u => u.BookRequested)
                                                .Include(u => u.DonorUser)
                                                .Where(x => x.BookRequestedId == bookId).ToList();

            //obter apenas o ganhador
            var winnerBookUser = bookUsers.Where(bu => bu.Status == DonationStatus.Donated).FirstOrDefault();

            //Book
            var book =  winnerBookUser.BookRequested;

            //usuarios que perderam a doação :(
            var losersBookUser = bookUsers.Where(bu => bu.Status == DonationStatus.Denied).ToList();

            //enviar e-mails
           await this._bookUsersEmailService.SendEmailDonationDeclined(book, winnerBookUser, losersBookUser);

        }

        public void NotifyUsersBookCanceled(Book book){

            
            List<BookRequest> bookUsers = _bookUserRepository.Get()
                                            .Include(u => u.DonorUser)
                                            .Where(x => x.BookRequestedId == book.Id).ToList();

            
            this._bookUsersEmailService.SendEmailDonationCanceled(book, bookUsers).Wait();

        }

        public void InformTrackingNumber(Guid bookId, string trackingNumber)
        {
            var book = _bookRepository.Get()
                                      .Include(d => d.User)
                                      .Include(f => f.UserFacilitator)
                                      .FirstOrDefault(id => id.Id == bookId);
            var winnerBookUser = _bookUserRepository
                                        .Get()
                                        .Include(u => u.DonorUser)
                                        .Where(bu => bu.BookRequestedId == bookId && bu.Status == DonationStatus.Donated)
                                        .FirstOrDefault();

            if (winnerBookUser == null)
                throw new ShareBookException("Vencedor ainda não foi escolhido");

            if(MuambatorConfigurator.IsActive)
                _muambatorService.AddPackageToTrackerAsync(book, winnerBookUser.DonorUser, trackingNumber);

            book.Status = BookStatus.Sent;
            book.TrackingNumber = trackingNumber; 
            _bookService.Update(book);

            // TODO: verificar se a notificação do muambator já é suficiente e remover esse trecho.
            if (winnerBookUser.DonorUser.AllowSendingEmail)
                //Envia e-mail para avisar o ganhador do tracking number                          
                _bookUsersEmailService.SendEmailTrackingNumberInformed(winnerBookUser, book).Wait();
        }
    }
}
