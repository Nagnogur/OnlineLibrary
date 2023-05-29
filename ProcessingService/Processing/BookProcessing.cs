using Microsoft.EntityFrameworkCore;
using ProcessingService.Entities;

namespace ProcessingService.Processing
{
    public class BookProcessing
    {
        public async Task<bool> SaveBook(LibraryContext db, Book book)
        {
            Book newBook = new Book();

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
            
            newBook.TimeRetrieved = book.TimeRetrieved;

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
    }
}
