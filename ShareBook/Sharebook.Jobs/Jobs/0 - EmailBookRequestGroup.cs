using ShareBook.Domain;
using ShareBook.Domain.Enums;
using ShareBook.Repository;
using ShareBook.Service;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Sharebook.Jobs.Jobs
{
    public class EmailBookRequestGroup : GenericJob, IJob
    {
        private readonly IBookRequestRepository _bookRequestRepository;
        private readonly IBookUsersEmailService _bookUsersEmailService;
        private readonly IUserRepository _userRepository;
        private int totalEmails;

        public EmailBookRequestGroup(
            IJobHistoryRepository jobHistoryRepo, IBookRequestRepository bookUserRepository,
            IBookUsersEmailService bookUsersEmailService, IUserRepository userRepository
            ) : base(jobHistoryRepo)
        {
            JobName = "RequestBooksNotify";
            Description = "Os requests de livros são enviados para o dono do livro, agrupando por intervalo e dono de livro, assim reduzindo a carga de emails enviados.";
            Interval = Interval.Hourly;
            Active = true;
            BestTimeToExecute = null;
            _userRepository = userRepository;
            _bookRequestRepository = bookUserRepository;
            _bookUsersEmailService = bookUsersEmailService;
        }

        public override JobHistory Work()
        {
            DateTime timeReference = DateTime.Now.AddHours(-1);

            List<BookRequestGroup> bookRequestGroups = new List<BookRequestGroup>();

            List<BookRequest> booksRequests = _bookRequestRepository.Get().Where(b => b.CreationDate >= timeReference && b.CreationDate <= DateTime.Now).ToList();

            List<User> differentDonors = booksRequests.Select(p => p.DonorUser).Distinct().ToList();
            List<User> differentRequestUsers = booksRequests.Select(p => p.RequestUser).Distinct().ToList();
            foreach (User donor in differentDonors)
            {
                bookRequestGroups.Add(new BookRequestGroup()
                {
                    Donor = donor
                });
            }
            
            //Filtrar request por doador e livro
            foreach (BookRequestGroup bookRequestGroup in bookRequestGroups)
            {
                List<BookRequest> result = booksRequests.Where(book => book.BookRequested.User.Id == bookRequestGroup.Donor.Id).ToList();

                bookRequestGroup.BookRequests = result;
               
                
            }

            if (booksRequests.Count > 0)
            {
            }

            return new JobHistory()
            {
                JobName = JobName,
                IsSuccess = true,
                Details = String.Join("\n", $"{totalEmails} e-mails enviados.")
            };
        }
    }
}