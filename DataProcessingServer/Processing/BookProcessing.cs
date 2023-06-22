using FuzzySharp;
using Microsoft.EntityFrameworkCore;
using ProcessingService.Entities;
using static System.Reflection.Metadata.BlobBuilder;

namespace ProcessingService.Processing
{
    public class BookProcessing
    {
        double updateAfter = 3;
        public async Task<bool> SaveBook(LibraryContext db, Book book)
        {
            Book newBook = new Book();

            if (book.Origin == null || book.Origin.Where(o => o.Link == null).Any())
            {
                return false;
            }

            newBook.TimeUpdated = DateTime.Now;

            newBook.Title = book.Title;
            newBook.Subtitle = book.Subtitle;
            newBook.AverageRating = book.AverageRating;
            newBook.Description = book.Description;
            newBook.Language = book.Language;
            newBook.MaturityRating = book.MaturityRating;
            newBook.PageCount = book.PageCount;
            newBook.PublishedDate = book.PublishedDate;
            newBook.Publisher = book.Publisher;
            newBook.RatingCount = book.RatingCount;
            newBook.ThumbnailFile = book.ThumbnailFile;

            newBook.TimeRetrieved = DateTime.Now;

            if (book.ThumbnailLink != null && book.ThumbnailLink.StartsWith("http"))
            {
                newBook.ThumbnailLink = book.ThumbnailLink;
            }

            foreach (Category c in book.Categories)
            {
                Category? category = await db.Categories.FirstOrDefaultAsync(u => u.CategoryName == c.CategoryName);
                if (category == null)
                {
                    try
                    {
                        var addedCat = db.Categories.Add(c);
                        await db.SaveChangesAsync();
                        newBook.Categories.Add(addedCat.Entity);
                    }
                    catch
                    {
                        ///

                    }
                }
                else
                {
                    newBook.Categories.Add(category);
                }
            }

            if (book.Authors != null)
            {
                newBook.Authors = book.Authors;
            }

            if (book.Origin != null)
            {
                newBook.Origin = book.Origin;
            }

            if (book.IndustryIdentifiers != null)
            {
                newBook.IndustryIdentifiers = book.IndustryIdentifiers;
            }


            try
            {
                db.Books.Add(newBook);
                var res = await db.SaveChangesAsync();
                if (res > 0)
                {
                    return true;
                }
                return false;
            }
            catch
            {
                return false;
            }
        }
        public async Task<bool> RateBook(LibraryContext db, Review review)
        {
            var book = await db.Books.Include(b => b.Reviews).FirstOrDefaultAsync(b => b.BookId == review.BookId);
            if (book == null)
            {
                return false;
            }

            var oldReview = book.Reviews.FirstOrDefault(r => r.UserId == review.UserId);

            if (oldReview == null)
            {
                db.Reviews.Add(review);
                var res = await db.SaveChangesAsync();
                return res > 0;
            }
            else
            {
                oldReview.Rating = review.Rating;
                db.Reviews.Update(oldReview);
                var res = await db.SaveChangesAsync();
                return res > 0;
            }
        }

        public async Task<bool> UpdateBook(LibraryContext db, Book newBook, Book bookToUpdate)
        {
            foreach (var link in newBook.Origin)
            {
                var originToUpdate = bookToUpdate.Origin.Where(o => o.Link == link.Link).FirstOrDefault();
                if (originToUpdate == null)
                {
                    bookToUpdate.Origin.Add(link);
                }
                else
                {
                    originToUpdate = link;
                }
            }

            if (bookToUpdate.TimeUpdated != null && DateTime.Now.Subtract(bookToUpdate.TimeUpdated.Value).TotalDays <= updateAfter)
            {
                return true;
            }

            db.Books.ExecuteUpdate(s => s
                .SetProperty(e => e.TimeUpdated, e => DateTime.Now));
            //bookToUpdate.TimeUpdated = DateTime.Now;


            bookToUpdate.Subtitle ??= newBook.Subtitle;
            bookToUpdate.AverageRating ??= newBook.AverageRating;
            bookToUpdate.Description ??= newBook.Description;
            bookToUpdate.Language ??= newBook.Language;
            bookToUpdate.MaturityRating ??= newBook.MaturityRating;
            bookToUpdate.PageCount ??= newBook.PageCount;
            bookToUpdate.PublishedDate ??= newBook.PublishedDate;
            bookToUpdate.Publisher ??= newBook.Publisher;
            bookToUpdate.RatingCount ??= newBook.RatingCount;
            bookToUpdate.ThumbnailFile ??= newBook.ThumbnailFile;

            //bookToUpdate.TimeRetrieved = DateTime.Now;
            //bookToUpdate.TimeUpdated = DateTime.Now;

            if (bookToUpdate.ThumbnailLink == null && newBook.ThumbnailLink != null && newBook.ThumbnailLink.StartsWith("http"))
            {
                bookToUpdate.ThumbnailLink = newBook.ThumbnailLink;
            }

            if (newBook.PageCount > bookToUpdate.PageCount)
            {
                bookToUpdate.PageCount = newBook.PageCount;
            }

            if (newBook.RatingCount > bookToUpdate.RatingCount)
            {
                bookToUpdate.RatingCount = newBook.RatingCount;
                bookToUpdate.AverageRating = newBook.AverageRating;
            }


            // category

            foreach (var author in newBook.Authors)
            {
                var existingAuthors = bookToUpdate.Authors;
                var extractedMatch = Process.ExtractOne(author.Name, existingAuthors.Select(a => a.Name), (s) => s);

                if (extractedMatch == null || extractedMatch.Score < 90)
                {
                    bookToUpdate.Authors.Add(author);
                }
            }

            if (bookToUpdate.IndustryIdentifiers == null)
            {
                bookToUpdate.IndustryIdentifiers = newBook.IndustryIdentifiers;
            }


            try
            {
                /*db.Books.Update(bookToUpdate);
                var res = await db.SaveChangesAsync();
                if (res > 0)
                {
                    return true;
                }
                return false;*/
                db.Books.Update(bookToUpdate);
                db.SaveChangesAsync();

                return true;
            }
            catch
            {
                return false;
            }
        }

        public async Task<bool> UpdateBookWithoutChecks(LibraryContext db, Book newBook, Book bookToUpdate, string? newTitle = null)
        {
            if (newTitle != null)
            {
                bookToUpdate.Title = newTitle;
            }

            db.Books.ExecuteUpdate(s => s
                .SetProperty(e => e.TimeUpdated, e => DateTime.Now));

            bookToUpdate.Subtitle = newBook.Subtitle;
            bookToUpdate.AverageRating = newBook.AverageRating;
            bookToUpdate.Description = newBook.Description;
            bookToUpdate.Language = newBook.Language;
            bookToUpdate.MaturityRating = newBook.MaturityRating;
            bookToUpdate.PageCount = newBook.PageCount;
            bookToUpdate.PublishedDate = newBook.PublishedDate;
            bookToUpdate.Publisher = newBook.Publisher;
            bookToUpdate.RatingCount = newBook.RatingCount;
            bookToUpdate.ThumbnailFile = newBook.ThumbnailFile;
            bookToUpdate.ThumbnailLink = newBook.ThumbnailLink;
            bookToUpdate.Authors = newBook.Authors;
            bookToUpdate.Categories = newBook.Categories;
            bookToUpdate.IndustryIdentifiers = newBook.IndustryIdentifiers;
            bookToUpdate.Origin = newBook.Origin;

            try
            {
                db.Books.Update(bookToUpdate);
                var res = await db.SaveChangesAsync();
                return res > 0;
            }
            catch
            {
                return false;
            }
        }

        public async Task<bool> DeleteBook(LibraryContext db, string title)
        {
            var bookToDelete = await db.Books.FirstOrDefaultAsync(b => b.Title == title);

            if (bookToDelete != null)
            {
                db.Books.Remove(bookToDelete);
                try
                {
                    var res = await db.SaveChangesAsync();
                    return res > 0;
                }
                catch
                {
                    return false;
                }
            }
            return false;
        }

        public async Task<bool> DeleteBook(LibraryContext db, int bookId, string userId)
        {
            var bookToDelete = await db.Books.FirstOrDefaultAsync(b => b.BookId == bookId && b.UserId == userId);

            if (bookToDelete != null)
            {
                db.Books.Remove(bookToDelete);
                try
                {
                    var res = await db.SaveChangesAsync();
                    return res > 0;
                }
                catch
                {
                    return false;
                }
            }
            return false;
        }

        public async Task<bool> SaveUserBook(LibraryContext db, Book book)
        {
            Book newBook = new Book();

            if (book.Origin == null || !book.Origin.Any() || book.Origin.Where(o => o.Link == null).Any())
            {
                return false;
            }

            if (book.UserId == null)
            {
                return false;
            }

            newBook.UserId = book.UserId;

            if (db.Books.Where(b => b.Title == book.Title && b.UserId == book.UserId).Any())
            {
                return false;
            }


            newBook.TimeUpdated = DateTime.Now;

            newBook.Title = book.Title;
            newBook.Subtitle = book.Subtitle;
            newBook.AverageRating = book.AverageRating;
            newBook.Description = book.Description;
            newBook.Language = book.Language;
            newBook.MaturityRating = book.MaturityRating;
            newBook.PageCount = book.PageCount;
            newBook.PublishedDate = DateTime.Now;
            newBook.Publisher = book.Publisher;
            newBook.RatingCount = book.RatingCount;
            newBook.ThumbnailFile = book.ThumbnailFile;
            newBook.FileLocation = book.FileLocation;

            newBook.TimeRetrieved = DateTime.Now;

            if (book.ThumbnailLink != null && book.ThumbnailLink.StartsWith("http"))
            {
                newBook.ThumbnailLink = book.ThumbnailLink;
            }
            if (book.ThumbnailFile != null && book.ThumbnailFile.Length > 10)
            {
                newBook.ThumbnailFile = book.ThumbnailFile;
            }

            foreach (Category c in book.Categories)
            {
                Category? category = await db.Categories.FirstOrDefaultAsync(u => u.CategoryName == c.CategoryName);
                if (category == null)
                {
                    try
                    {
                        var addedCat = db.Categories.Add(c);
                        await db.SaveChangesAsync();
                        newBook.Categories.Add(addedCat.Entity);
                    }
                    catch
                    {
                        ///

                    }
                }
                else
                {
                    newBook.Categories.Add(category);
                }
            }

            if (book.Authors != null)
            {
                newBook.Authors = book.Authors;
            }

            if (book.Origin != null)
            {
                newBook.Origin = book.Origin;
            }

            if (book.IndustryIdentifiers != null)
            {
                newBook.IndustryIdentifiers = book.IndustryIdentifiers;
            }


            try
            {
                db.Books.Add(newBook);
                var res = await db.SaveChangesAsync();
                if (res > 0)
                {
                    return true;
                }
                return false;
            }
            catch
            {
                return false;
            }
        }

        public async Task<bool> EditUserBook(LibraryContext db, Book book)
        {
            var bookToUpdate = await db.Books
                .Where(b => b.BookId == book.BookId)
                .Include(b => b.Authors)
                .Include(b => b.Origin)
                .Include(b => b.IndustryIdentifiers)
                .FirstOrDefaultAsync();

            if (bookToUpdate == null)
            {
                return false;
            }


            bookToUpdate.TimeUpdated = DateTime.Now;

            bookToUpdate.Title = book.Title;
            bookToUpdate.Subtitle = book.Subtitle;
            bookToUpdate.Description = book.Description;
            bookToUpdate.Language = book.Language;
            bookToUpdate.MaturityRating = book.MaturityRating;
            bookToUpdate.PageCount = book.PageCount;
            bookToUpdate.Publisher = book.Publisher;
            if (book.ThumbnailFile != null && book.ThumbnailFile.Length > 10)
            {
                bookToUpdate.ThumbnailFile = book.ThumbnailFile;
            }
            if (book.FileLocation != null)
            {
                bookToUpdate.FileLocation = book.FileLocation;
            }


            try
            {
                db.Books.Update(bookToUpdate);
                var res = await db.SaveChangesAsync();
                if (res > 0)
                {
                    return true;
                }
                return false;
            }
            catch
            {
                return false;
            }
        }
    }
}
