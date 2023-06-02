using FuzzySharp;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using ProcessingService;
using ProcessingService.Entities;
using ProcessingService.Processing;
using ProcessingService.RetrieveLogic;

namespace DataProcessingServer.Controllers
{
    [Route("api")]
    [ApiController]
    public class BookController : ControllerBase
    {
        private LibraryContext db;
        public BookController(LibraryContext libraryContext)
        {
            db = libraryContext;
        }
        [HttpGet]
        [Route("books")]
        public async Task<IEnumerable<Book>> GetBooksWithParameters([FromQuery] BookQueryParameters parameters)
        {
            BookManipulationService bookManipulationService = new BookManipulationService(db);
            var res = await bookManipulationService.GetBooksWithParameters(parameters);

            return res;
        }

        [HttpPut]
        [Route("updatebook")]
        public async Task<IActionResult> UpdateBook(Book book)
        {
            BookProcessing bookProcessing = new BookProcessing();
            var bookToUpdate = await db.Books
                .Where(b => b.Title == book.Title)
                .Include(b => b.Authors)
                .Include(b => b.Origin)
                .Include(b => b.IndustryIdentifiers)
                .FirstOrDefaultAsync();
            
            if (bookToUpdate == null)
            {
                return NotFound();
            }

            var res = await bookProcessing.UpdateBook(db, book, bookToUpdate);

            if (res)
            {
                return Ok();
            }
            else
            {
                return BadRequest();
            }
        }

        [HttpPost]
        [Route("addbooks")]
        public async Task<IActionResult> AddBooks(List<Book> books)
        {
            int res = 0;
            var bookTitles = db.Books.Select(b => b.Title).ToList();
            BookProcessing bookProcessing = new BookProcessing();

            foreach (var book in books)
            {
                /*if (book.Title.Contains("pleen"))
                {

                }*/

                
                var extractedMatch = Process.ExtractOne(book.Title, bookTitles, (s) => s);

                bookTitles.Add(book.Title);

                if (extractedMatch == null)
                {
                    var isSaved = await bookProcessing.SaveBook(db, book);
                    if (isSaved)
                    {
                        res++;
                    }
                    continue;
                }

                if (extractedMatch.Score >= 90)
                {
                    Book? extractedBook = await db.Books
                    .Include(b => b.Authors)
                    .Include(b => b.Origin)
                    .FirstOrDefaultAsync(b => b.Title == extractedMatch.Value);

                    if (extractedBook == null)
                    {
                        continue;
                    }

                    var bookLink = book.Origin.FirstOrDefault();
                    if (extractedBook.Origin.Where(o => o.Link == bookLink?.Link).Any())
                    {
                        bookProcessing.UpdateBook(db, book, extractedBook);
                        continue;
                    }
                    /*else if (bookLink != null)
                    {
                        // set toUpdate
                        extractedBook.Origin.Add(bookLink);
                    }*/

                    if (extractedBook.Authors == null || book.Authors == null)
                    {
                        /// set new book or update old?
                        try
                        {
                            bookProcessing.UpdateBook(db, book, extractedBook);
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

                        if (extractedAuthorMatch.Score < 85)
                        {
                            /// 
                            var isSaved = await bookProcessing.SaveBook(db, book);
                            if (isSaved)
                            {
                                res++;
                            }
                            continue;
                        }
                        else
                        {
                            bookProcessing.UpdateBook(db, book, extractedBook);
                            continue;
                        }
                    }
                }
                else
                {
                    var isSaved = await bookProcessing.SaveBook(db, book);
                    if (isSaved)
                    {
                        res++;
                    }
                }
            }

            return Ok(res);
        }

        [HttpPost]
        [Route("forceaddbook")]
        public async Task<IActionResult> ForceAdd(Book book)
        {
            BookProcessing bookProcessing = new BookProcessing();

            var isSaved = await bookProcessing.SaveBook(db, book);
            if (isSaved)
            {
                return Ok();
            }

            return BadRequest();
        }

        [HttpDelete]
        [Route("deleteBook")]
        public async Task<IActionResult> Delete(string title)
        {
            BookProcessing bookProcessing = new BookProcessing();

            var deleted = await bookProcessing.DeleteBook(db, title);
            if (deleted)
            {
                return Ok();
            }

            return BadRequest();
        }

        [HttpPut]
        [Route("forceupdate")]
        public async Task<IActionResult> ForceUpdate(Book book, string? title = null)
        {
            BookProcessing bookProcessing = new BookProcessing();

            var bookToUpdate = await db.Books
                .Where(b => b.Title == book.Title)
                .Include(b => b.Authors)
                .Include(b => b.Origin)
                .Include(b => b.IndustryIdentifiers)
                .FirstOrDefaultAsync();

            if (bookToUpdate == null)
            {
                return NotFound();
            }

            var updated = await bookProcessing.UpdateBookWithoutChecks(db, book, bookToUpdate, title);
            if (updated)
            {
                return Ok();
            }

            return BadRequest();
        }

    }
}
