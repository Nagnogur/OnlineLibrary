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

        /// <returns>  false if book does not exist </returns>
        /*public async Task<bool> Exist(LibraryContext db, Book book, List<string> bookTitles)
        {
            var extractedMatch = Process.ExtractOne(book.Title, bookTitles, (s) => s);
            if (extractedMatch == null)
            {
                return false;
            }

            if (extractedMatch.Score >= 90)
            {
                Book? extractedBook = await db.Books
                .Include(b => b.Authors)
                .Include(b => b.Origin)
                .FirstOrDefaultAsync(b => b.Title == extractedMatch.Value);

                if (extractedBook == null)
                {
                    return true;
                }

                var bookLink = book.Origin.FirstOrDefault();
                if (extractedBook.Origin.Where(o => o.Link == bookLink?.Link).Any())
                {
                    ///  set missing parameters
                    return false;
                }
                else if (bookLink != null)
                {
                    extractedBook.Origin.Add(bookLink);
                }

                if (extractedBook.Authors == null || book.Authors == null)
                {
                    /// set new book
                    try
                    {
                        db.Books.Update(extractedBook);
                        await db.SaveChangesAsync();
                        continue;
                    }
                    catch (Exception ex)
                    {
                        continue;
                    }
                }

                foreach (var author in book.Authors)
                {
                    var extractedAuthorMatch = Process.ExtractOne(author.Name, extractedBook.Authors.Select(a => a.Name), (s) => s);

                    if (extractedAuthorMatch.Score < 90)
                    {
                        /// 
                        var isSaved = await bookProcessing.SaveBook(db, book);
                        if (isSaved)
                        {
                            res++;
                            bookTitles.Add(book.Title);
                        }
                        continue;
                    }
                    else
                    {
                        try
                        {
                            db.Books.Update(extractedBook);
                            await db.SaveChangesAsync();
                            continue;
                        }
                        catch (Exception ex)
                        {
                            continue;
                        }
                    }
                }
            }
            else
            {
                var isSaved = await bookProcessing.SaveBook(db, book);
                if (isSaved)
                {
                    res++;
                    bookTitles.Add(book.Title);
                }
            }
        }*/
    }
}
