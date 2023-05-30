using FuzzySharp;
using FuzzySharp.SimilarityRatio.Scorer.StrategySensitive;
using FuzzySharp.SimilarityRatio;
using Microsoft.EntityFrameworkCore;
using ProcessingService.Entities;
using FuzzyString;

namespace ProcessingService.RetrieveLogic
{
    public class BookManipulationService
    {
        private LibraryContext db;

        private int cutoff = 80;
        //private int limit = 20;
        
        public BookManipulationService(LibraryContext db)
        {
            this.db = db;
        }
        public async Task<IEnumerable<Book>> GetBooksWithParameters(BookQueryParameters parameters)
        {
            
            IQueryable<Book> books = db.Books
                .AsNoTracking()
                .Include(b => b.Authors)
                .Include(b => b.Categories)
                .Include(b => b.IndustryIdentifiers)
                .Include(b => b.Origin);

            if (parameters.Publisher != null)
            {
                books = FilterByPublisher(books, parameters.Publisher);
            }
            if (parameters.MinRating != null || parameters.MaxRating != null)
            {
                books = FilterByRating(books, parameters.MinRating, parameters.MaxRating);
            }
            
            if (parameters.MinPrice != null)
            {
                books = FilterByMinPrice(books, parameters.MinPrice.Value);
            }

            if (parameters.MaxPrice != null)
            {
                books = FilterByMaxPrice(books, parameters.MaxPrice.Value);
            }

            if (parameters.IdentifierCode != null)
            {
                books = FilterByIdentifierCode(books, parameters.IdentifierCode);
            }

            if (parameters.MaxPageCount != null)
            {
                books = FilterByMaxPageCount(books, parameters.MaxPageCount.Value);
            }

            if (parameters.MinPageCount != null)
            {
                books = FilterByMinPageCount(books, parameters.MinPageCount.Value);
            }

            if (parameters.Domain != null)
            {
                books = FilterByDomain(books, parameters.Domain);
            }

            if (parameters.Author != null)
            {
                books = FilterByAuthor(books, parameters.Author);
            }

            if (parameters.MinPublishDate != null)
            {
                books = FilterByMinPublishDate(books, parameters.MinPublishDate.Value);
            }

            if (parameters.MaxPublishDate != null)
            {
                books = FilterByMaxPublishDate(books, parameters.MaxPublishDate.Value);
            }

            if (parameters.MinRetrievedDate != null)
            {
                books = FilterByMinRetrieveDate(books, parameters.MinRetrievedDate.Value);
            }

            if (parameters.MaxRetrievedDate != null)
            {
                books = FilterByMaxRetrieveDate(books, parameters.MaxRetrievedDate.Value);
            }

            if (parameters.WithSeveralLinks != null)
            {
                books = FilterByLinkCount(books, parameters.WithSeveralLinks.Value);
            }
            if (parameters.WithDiscount != null)
            {
                books = FilterByDiscount(books, parameters.WithDiscount.Value);
            }

            /// ------------------
            if (parameters.Title != null)
            {
                books = FilterByTitle(books, parameters.Title);
            }

            books = books
                .OrderBy(b => b.Title)
                .Skip((parameters.PageNumber - 1) * parameters.PageSize)
                .Take(parameters.PageSize);

            return await books.ToListAsync();
        }

        public IEnumerable<Book> FilterByTitleLCS(IEnumerable<Book> books, string userTitle, int tolerance = 1)
        {
            var matchingBooks = books
                .Where(b => b.Title.ToLower().Contains(userTitle)
            || b.Title.LongestCommonSubsequence(userTitle).Length >= (userTitle.Length - tolerance));

            return matchingBooks;
        }


        /// Under constraction
        public IQueryable<Book> FilterByTitle(IQueryable<Book> books, string userTitle)
        {
            var bookTitles = books.Select(b => b.Title).ToList();
            var extractedMatches = Process.ExtractAll(userTitle, bookTitles, cutoff: cutoff, scorer: ScorerCache.Get<PartialTokenSetScorer>());//, limit: limit);

            if (extractedMatches == null)
            {
                return Enumerable.Empty<Book>().AsQueryable();
            }

            IQueryable<Book> query = books.Where(b => b.Title.Contains(userTitle) 
                || extractedMatches.Select(em => em.Value).Contains(b.Title));

            return query;
        }

        public IQueryable<Book> FilterByPublisher(IQueryable<Book> books, string userPublisher)
        {
            /*var bookTitles = db.Books.Select(b => b.Title).ToList();
            var extractedMatches = Process.ExtractAll(userTitle, bookTitles, cutoff: cutoff, scorer: ScorerCache.Get<PartialTokenSetScorer>());//, limit: limit);

            if (extractedMatches == null)
            {
                return Enumerable.Empty<Book>().AsQueryable();
            }

            IQueryable<Book> query = books.Where(b => b.Title.Contains(userTitle)
            ||
            extractedMatches.Select(em => em.Value).Contains(b.Title));*/

            IQueryable<Book> query = books.Where(b => b.Publisher != null && b.Publisher.Contains(userPublisher));

            return query;
        }

        public IQueryable<Book> FilterByRating(IQueryable<Book> books, float? minRating = 0, float? maxRating = 10)
        {
            IQueryable<Book> query = books.Where(b => b.AverageRating != null 
            && b.AverageRating >= minRating && b.AverageRating <= maxRating);

            return query;
        }
        public IQueryable<Book> FilterByMinPrice(IQueryable<Book> books, float minPrice)
        {
            IQueryable<Book> query = books.Where(b => b.Origin != null
            && b.Origin.Where(o => o.RetailPrice >= minPrice).Any());

            return query;
        }
        public IQueryable<Book> FilterByMaxPrice(IQueryable<Book> books, float maxPrice)
        {
            IQueryable<Book> query = books.Where(b => b.Origin != null
            && b.Origin.Where(o => !o.RetailPrice.HasValue || o.RetailPrice <= maxPrice).Any());

            return query;
        }
        public IQueryable<Book> FilterByIdentifierCode(IQueryable<Book> books, string code)
        {
            IQueryable<Book> query = books.Where(b => b.IndustryIdentifiers != null
            && b.IndustryIdentifiers.Where(i => i.IdentifierCode == code).Any());

            return query;
        }
        public IQueryable<Book> FilterByMinPageCount(IQueryable<Book> books, int minPageCount)
        {
            IQueryable<Book> query = books.Where(b => b.PageCount != null && b.PageCount >= minPageCount);

            return query;
        }
        public IQueryable<Book> FilterByMaxPageCount(IQueryable<Book> books, int maxPageCount)
        {
            IQueryable<Book> query = books.Where(b => b.PageCount <= maxPageCount);

            return query;
        }
        public IQueryable<Book> FilterByDomain(IQueryable<Book> books, string domain)
        {
            IQueryable<Book> query = books.Where(b => 
            b.Origin != null 
            && b.Origin.Where(o => o.PortalDomain != null && o.PortalDomain.Contains(domain)).Any());

            return query;
        }
        public IQueryable<Book> FilterByAuthor(IQueryable<Book> books, string author)
        {
            IQueryable<Book> query = books.Where(b =>
            b.Authors.Any() && b.Authors.Where(a => a.Name.Contains(author)).Any());

            return query;
        }
        public IQueryable<Book> FilterByMinPublishDate(IQueryable<Book> books, DateTime minPublishDate)
        {
            IQueryable<Book> query = books.Where(b => b.PublishedDate >= minPublishDate);

            return query;
        }
        public IQueryable<Book> FilterByMaxPublishDate(IQueryable<Book> books, DateTime maxPublishDate)
        {
            IQueryable<Book> query = books.Where(b => b.PublishedDate <= maxPublishDate);

            return query;
        }
        public IQueryable<Book> FilterByMinRetrieveDate(IQueryable<Book> books, DateTime minRetrieveDate)
        {
            IQueryable<Book> query = books.Where(b => b.TimeRetrieved >= minRetrieveDate);

            return query;
        }
        public IQueryable<Book> FilterByMaxRetrieveDate(IQueryable<Book> books, DateTime maxRetrieveDate)
        {
            IQueryable<Book> query = books.Where(b => b.TimeRetrieved <= maxRetrieveDate);

            return query;
        }
        public IQueryable<Book> FilterByLinkCount(IQueryable<Book> books, bool several)
        {
            IQueryable<Book> query;
            if (several)
            {
                query = books.Where(b => b.Origin.Count() > 1);
            }
            else
            {
                query = books.Where(b => b.Origin.Count() == 1);
            }

            return query;
        }
        public IQueryable<Book> FilterByDiscount(IQueryable<Book> books, bool withDiscount)
        {
            IQueryable<Book> query = books;
            if (withDiscount)
            {
                query = books.Where(b => b.Origin.Where(o => o.RetailPrice < o.ListPrice).Any()
                || b.Origin.Where(o => o.RetailPrice != null).Distinct().Count() > 1);
            }

            return query;
        }
    }
}
