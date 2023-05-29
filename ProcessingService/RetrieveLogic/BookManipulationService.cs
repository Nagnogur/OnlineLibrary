using FuzzySharp;
using Microsoft.EntityFrameworkCore;
using ProcessingService.Entities;

namespace ProcessingService.RetrieveLogic
{
    public class BookManipulationService
    {
        private LibraryContext db;

        private int cutoff = 90;

        public BookManipulationService(LibraryContext db)
        {
            this.db = db;
        }
        public async Task<IEnumerable<Book>> GetBooksWithParameters(BookQueryParameters parameters)
        {
            IQueryable<Book> books = db.Books
                .Include(b => b.Authors)
                .Include(b => b.Categories)
                .Include(b => b.IndustryIdentifiers)
                .Include(b => b.Origin);

            books = FilterByTitle(books, parameters);

            /*.OrderBy(b => b.Title)
            .Skip((parameters.pageNumber - 1) * parameters.PageSize)
            .Take(parameters.PageSize)
            .ToListAsync();*/

            /*books = books
                .Skip((parameters.pageNumber - 1) * parameters.PageSize)
                .Take(parameters.PageSize);*/

            return await books.ToListAsync();
        }

        public IQueryable<Book> FilterByTitle (IQueryable<Book> books, BookQueryParameters parameters)
        {
            if (parameters.Title == null) 
            {
                return books;
            }
            var bookTitles = db.Books.Select(b => b.Title).ToList();
            var extractedMatches = Process.ExtractAll(parameters.Title, bookTitles, cutoff: cutoff);

            if (extractedMatches == null)
            {
                return Enumerable.Empty<Book>().AsQueryable();
            }

            IQueryable<Book> query = books.Where(b => extractedMatches.Select(em => em.Value).Contains(b.Title));

            return query;
        }
    }
}
