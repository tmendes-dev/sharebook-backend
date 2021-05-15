using ShareBook.Domain;

namespace ShareBook.Repository
{
    public class BookUserRepository : RepositoryGeneric<BookRequest>, IBookRequestRepository
    {
        public BookUserRepository(ApplicationDbContext context) : base(context)
        {
        }
    }
}
