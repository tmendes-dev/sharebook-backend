using ShareBook.Domain.Common;
using System.Collections.Generic;

namespace ShareBook.Domain
{
    public class BookRequestGroup : BaseEntity
    {
        public User Donor { get; set; }

        public List<Book> DonorBooks { get; set; }

        public List<BookRequest> BookRequests { get; set; }

        public List<User> RequestsUsers { get; set; }
    }
}