using AutoMapper;
using Gateway.Entities;
using Gateway.Mapper;
using Gateway.Models;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.WebUtilities;
using Newtonsoft.Json;
using System.Data;
using System.Net;
using System.Security.Claims;
using System.Text;

namespace Gateway.Controllers
{
    [Authorize(Roles = "User")]
    [Route("useractions")]
    [ApiController]
    public class UserActionsController : ControllerBase
    {
        private HttpClient client;
        private IHttpClientFactory clientFactory;
        private IMapper mapper;
        private readonly UserManager<IdentityUser> _userManager;

        MapperConfiguration mapperConfiguration = new MapperConfiguration(cfg =>
        {
            cfg.AddProfile<MapperProfile>();
        });
        public UserActionsController(HttpClient client, IHttpClientFactory clientFactory, UserManager<IdentityUser> userManager)
        {
            this.client = client;
            mapper = mapperConfiguration.CreateMapper();
            this.clientFactory = clientFactory;
            _userManager = userManager;
        }


        [HttpPost("create")]
        public async Task<IActionResult> AddBook([FromBody] BookModel book) // TODO add file
        {
            string processingServerUrl = "https://localhost:7248/api/saveuserbook";

            var userEmail = HttpContext.User.Identity?.Name;
            var user = await _userManager.FindByEmailAsync(userEmail);
            if (user == null)
            {
                return Unauthorized();
            }
            book.UserId = user.Id;

            if (!System.IO.File.Exists(book.FileLocation))
            {
                return BadRequest("No file provided");
            }

            Book newBook = mapper.Map<Book>(book);
            newBook.Origin.Add(new LinkPrice { Link = "OnlineLibrary\\"+newBook.FileLocation, PortalDomain="OnlineLibrary"}) ;
            newBook.Authors.Add(new Author { Name = user.Email});

            /*if (newBook.FileLocation == null)
            {
                return BadRequest("No file provided");
            }*/

            var jsonBody = JsonConvert.SerializeObject(newBook);
            var body = new StringContent(jsonBody, Encoding.UTF8, "application/json");
            var response = await client.PostAsync(processingServerUrl, body);
            if (response.IsSuccessStatusCode)
            {
                return Ok();
            }
            else
            {
                return BadRequest();
            }
        }

        [HttpPut("edit")]
        public async Task<IActionResult> EditBook([FromBody] BookModel book)
        {
            string processingServerUrl = "https://localhost:7248/api/edituserbook/" + book.BookId;

            var userEmail = HttpContext.User.Identity?.Name;
            var user = await _userManager.FindByEmailAsync(userEmail);
            if (user == null)
            {
                return Unauthorized();
            }
            var oldBook = await GetBookById(book.BookId);
            if (oldBook == null)
            {
                return NotFound();
            }
            if (oldBook.UserId != user.Id)
            {
                return Unauthorized();
            }

            Book newBook = mapper.Map<Book>(book);

            
            var jsonBody = JsonConvert.SerializeObject(newBook);
            var body = new StringContent(jsonBody, Encoding.UTF8, "application/json");
            var response = await client.PutAsync(processingServerUrl, body);
            if (response.IsSuccessStatusCode)
            {
                return Ok();
            }
            else
            {
                return BadRequest();
            }
        }

        [HttpDelete("{bookId}/delete")]
        public async Task<IActionResult> DeleteBook(int bookId)
        {
            string processingServerUrl = "https://localhost:7248/api/delete?bookId=" + bookId + "&userId=";

            var userEmail = HttpContext.User.Identity?.Name;
            var user = await _userManager.FindByEmailAsync(userEmail);
            if (user == null)
            {
                return Unauthorized();
            }

            processingServerUrl += user.Id;
            var response = await client.DeleteAsync(processingServerUrl);
            if (response.IsSuccessStatusCode)
            {
                return Ok();
            }
            else
            {
                return BadRequest();
            }
        }

        [HttpPost("savefile")]
        public async Task<IActionResult> SaveBookFile()//[FromForm]IFormFile bookFile)
        {
            try
            {
                var bookFile = Request.Form.Files.FirstOrDefault();
                if (bookFile.Length > 0)
                {
                    //var uniqueFileName = $"{Guid.NewGuid()}_{bookFile.FileName}";
                    var filePath = Path.Combine("BookFiles",
                        bookFile.FileName);

                    using (var stream = System.IO.File.Create(filePath))
                    {
                        await bookFile.CopyToAsync(stream);
                    }
                    return Ok(filePath);
                }
            }
            catch (Exception ex)
            {
                return BadRequest(ex);
            }

            return BadRequest();
        }

        [HttpPost("rate")]
        public async Task<IActionResult> RateBook([FromBody] RateModel review) // TODO add file
        {
            string processingServerUrl = "https://localhost:7248/api/ratebook";

            var userEmail = HttpContext.User.Identity?.Name;
            var user = await _userManager.FindByEmailAsync(userEmail);
            if (user == null)
            {
                return Unauthorized();
            }
            
                        
            Review newReview = new Review 
            { 
                UserId = user.Id,
                BookId = review.BookId,
                Rating = review.Rating,
            };

            var jsonBody = JsonConvert.SerializeObject(newReview);
            var body = new StringContent(jsonBody, Encoding.UTF8, "application/json");
            var response = await client.PostAsync(processingServerUrl, body);
            if (response.IsSuccessStatusCode)
            {
                await RetrainModel();
                return Ok();
            }
            else
            {
                return BadRequest();
            }
        }
        public class RateModel
        {
            public int BookId { get; set; }
            public int Rating { get; set; }
        }

        [HttpGet("getrecommended")]
        public async Task<IActionResult> GetRecommendations(int numberOfRecommendations = 3)
        {
            var url = "https://localhost:7275/recommender";

            var userEmail = HttpContext.User.Identity?.Name;
            var user = await _userManager.FindByEmailAsync(userEmail);
            if (user == null)
            {
                return Unauthorized();
            }

            var books = await GetUnratedBooks(user.Id);
            if (books.Count() == 0)
            {
                return NotFound();
            }

            var jsonBody = JsonConvert.SerializeObject(books);
            var body = new StringContent(jsonBody, Encoding.UTF8, "application/json");

            var response = await client.PostAsync(url + "/" + user.Id, body);
            if (response.IsSuccessStatusCode)
            {
                List<BookPrediction>? predictions = JsonConvert.DeserializeObject<List<BookPrediction>>(await response.Content.ReadAsStringAsync());

                List<BookModel> recommendedBooks = new List<BookModel>();
                foreach (var prediction in predictions)
                {
                    var book = await GetBookById(prediction.BookId);
                    if (book != null)
                    {
                        recommendedBooks.Add(book);
                    }
                }

                return Ok(recommendedBooks);
            }
            else
            {
                return BadRequest(response.StatusCode);
            }
        }

        private async Task<List<BookModel>> GetUnratedBooks(string userId)
        {
            var url = "https://localhost:7248/api/unratedbooks";

            var response = await client.GetAsync(url + "?userId=" + userId);

            if (response.IsSuccessStatusCode)
            {
                var res = JsonConvert.DeserializeObject<List<Book>>(await response.Content.ReadAsStringAsync());
                var mappedBooks = mapper.Map<List<BookModel>>(res);
                return mappedBooks;
            }
            else return new List<BookModel>();
        }
        public class BookPrediction
        {
            public int BookId { get; set; }
            public float RatingPrediction { get; set; }
        }

        private async Task RetrainModel()
        {
            var url = "https://localhost:7248/api/ratings";

            var response = await client.GetAsync(url);
            if (response.IsSuccessStatusCode)
            {
                var res = JsonConvert.DeserializeObject<List<Review>>(await response.Content.ReadAsStringAsync());

                var recommenderUrl = "https://localhost:7275/recommender";
                var recommendations = client.PostAsync(recommenderUrl, response.Content);
                return;
            }
            else
            {
                return;
            }
        }

        private async Task<BookModel?> GetBookById(int id)
        {
            var bookDataSourceClient = clientFactory.CreateClient("BookDataSourceClient");

            var url = bookDataSourceClient.BaseAddress;

            var response = await bookDataSourceClient.GetAsync(url + "/" + id);
            if (response.IsSuccessStatusCode)
            {
                if (response.StatusCode == HttpStatusCode.NoContent)
                {
                    return null;
                }
                var res = JsonConvert.DeserializeObject<Book>(await response.Content.ReadAsStringAsync());
                var mappedBooks = mapper.Map<BookModel>(res);
                return mappedBooks;
            }
            else
            {
                return null;
            }
        }
    }
}
