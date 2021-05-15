using ShareBook.Domain;
using System;
using System.Collections.Generic;
using System.Text;

namespace ShareBook.Test.Unit.Mocks
{
    public class BookUserMock
    {
        public static BookRequest GetDonation(Book book, User requestingUser)
        {
            return new  BookRequest()
            {
                BookRequested = book,
                DonorUser = requestingUser,
                Reason = "MOTIVO"
            };
        }
    }
}
