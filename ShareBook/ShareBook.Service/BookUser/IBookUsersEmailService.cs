using ShareBook.Domain;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace ShareBook.Service
{
    public interface IBookUsersEmailService
    {
        Task SendEmailBookRequested(BookRequest bookUser);

        Task SendEmailBookDonated(BookRequest bookUser);

        Task SendEmailBookDonatedNotifyDonor(Book book, User winner);

        Task SendEmailBookDonor(BookRequest bookUser, Book bookRequested);

        Task SendEmailBookInterested(BookRequest bookUser, Book book);

        Task SendEmailDonationDeclined(Book book, BookRequest bookUserWinner, List<BookRequest> bookUsersDeclined);

        Task SendEmailDonationCanceled(Book book, List<BookRequest> bookUsers);

        Task SendEmailBookCanceledToAdmins(Book book);

        Task SendEmailTrackingNumberInformed(BookRequest bookUserWinner, Book book);
        Task SendEmailMaxRequests(Book bookRequested);
    }
}
