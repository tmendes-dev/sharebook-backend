﻿using ShareBook.Domain;
using ShareBook.Domain.Common;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace ShareBook.Service
{
    public interface IBookUserService 
    {
        void Insert(Guid bookId, string reason);

        IList<User> GetGranteeUsersByBookId(Guid bookId);

        IList<BookRequest> GetRequestersList(Guid bookId);

        void DonateBook(Guid bookId, Guid userId, string note);

        void DeniedBookUsers(Guid bookId);

        PagedList<BookRequest> GetRequestsByUser(int page, int items);

        /// <summary>
        /// Comunicar os interessados não escolhidos sobre a finalização da doação. e quem ganhou o livro
        /// </summary>
        /// <param name="bookId"></param>
        Task NotifyInterestedAboutBooksWinner(Guid bookId);

        Result<Book> Cancel(Guid bookId, bool isAdmin = false);

        void InformTrackingNumber(Guid bookId, string trackingNumber);
    }
}
