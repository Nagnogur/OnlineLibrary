using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using ProcessingService.Entities;
using ProcessingService.RetrieveLogic;

namespace ProcessingService
{
    [Route("api/books")]
    [ApiController]
    public class BookController : ControllerBase
    {
        private LibraryContext db;
        public BookController(LibraryContext libraryContext)
        {
            this.db = libraryContext;
        }
        [HttpGet]
        public async Task<IEnumerable<Book>> GetBooksWithParameters([FromQuery] BookQueryParameters parameters)
        {
            BookManipulationService bookManipulationService = new BookManipulationService(db);
            var res = await bookManipulationService.GetBooksWithParameters(parameters);

            return res;
        }
    }
}
