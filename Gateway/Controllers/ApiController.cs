using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.WebUtilities;
using Newtonsoft.Json;
using ProcessingService.RetrieveLogic;
using System.Reflection.Metadata;
using System;
using System.Text;
using Gateway.Entities;
using AutoMapper;
using Gateway.Models;
using Gateway.Mapper;
using Microsoft.Extensions.Hosting;
using static System.Net.WebRequestMethods;
using Microsoft.Extensions.Configuration;
using Gateway.Extensions;
using System.Net;
using Azure;
using Microsoft.AspNetCore.StaticFiles;

// For more information on enabling Web API for empty projects, visit https://go.microsoft.com/fwlink/?LinkID=397860

namespace Gateway.Controllers
{
    [Route("api")]
    [ApiController]
    public class ApiController : ControllerBase
    {
        private HttpClient client;
        private IHttpClientFactory clientFactory;
        private IMapper mapper;

        //string orchestratorUrl = "https://localhost:7021/invokeservice/";
        
        MapperConfiguration configuration = new MapperConfiguration(cfg => {
            cfg.AddProfile<MapperProfile>();
        });

        public ApiController(HttpClient client, IHttpClientFactory clientFactory)
        {
            this.client = client;
            mapper = configuration.CreateMapper();
            this.clientFactory = clientFactory;
        }

        

        // GET api/<GatewayController>
        [HttpGet("books")]
        public async Task<IActionResult> Get([FromQuery] BookQueryParameters query)
        {
            //var sourceUrl = "https://localhost:7190/api";

            var bookDataSourceClient = clientFactory.CreateClient("BookDataSourceClient");

            var url = bookDataSourceClient.BaseAddress;
            string queryString = query.GetQueryString();

            var response = await bookDataSourceClient.GetAsync(url + "?" + queryString);
            if (response.IsSuccessStatusCode)
            {
                var res = JsonConvert.DeserializeObject<List<Book>>(await response.Content.ReadAsStringAsync());
                var mappedBooks = mapper.Map<List<BookModel>>(res);
                return Ok(mappedBooks);
            }
            else
            {
                return BadRequest(response.StatusCode);
            }
        }

        // GET api/<GatewayController>
        [HttpGet("books/{id}")]
        public async Task<IActionResult> GetById(int id)
        {
            var bookDataSourceClient = clientFactory.CreateClient("BookDataSourceClient");

            var url = bookDataSourceClient.BaseAddress;

            var response = await bookDataSourceClient.GetAsync(url + "/" + id);
            if (response.IsSuccessStatusCode)
            {
                if (response.StatusCode == HttpStatusCode.NoContent)
                {
                    return NotFound();
                }
                var res = JsonConvert.DeserializeObject<Book>(await response.Content.ReadAsStringAsync());
                var mappedBooks = mapper.Map<BookModel>(res);
                return Ok(mappedBooks);
            }
            else
            {
                return BadRequest(response.StatusCode);
            }
        }

        // GET api/7qrjt-gwk.../books
        [HttpGet("{id}/books")]
        public async Task<IActionResult> GetByUserId(string id)
        {
            // api/books
            var bookDataSourceClient = clientFactory.CreateClient("BookDataSourceClient");

            var url = bookDataSourceClient.BaseAddress;

            // api/books/user/id    
            var response = await bookDataSourceClient.GetAsync(url + "/user/" + id);
            if (response.IsSuccessStatusCode)
            {
                if (response.StatusCode == HttpStatusCode.NoContent)
                {
                    return NotFound();
                }
                var res = JsonConvert.DeserializeObject<List<Book>>(await response.Content.ReadAsStringAsync());
                var mappedBooks = mapper.Map<List<BookModel>>(res);
                return Ok(mappedBooks);
            }
            else
            {
                return BadRequest(response.StatusCode);
            }
        }

        [HttpGet("download/{bookId}")]
        public async Task<IActionResult> Download(int bookId)
        {
            var bookDataSourceClient = clientFactory.CreateClient("BookDataSourceClient");

            var url = bookDataSourceClient.BaseAddress;

            var response = await bookDataSourceClient.GetAsync(url + "/" + bookId);
            if (response.IsSuccessStatusCode)
            {
                if (response.StatusCode == HttpStatusCode.NoContent)
                {
                    return NotFound();
                }
                var book = JsonConvert.DeserializeObject<Book>(await response.Content.ReadAsStringAsync());
                
                if (book == null)
                {
                    return NotFound();
                }

                var filePath = book.FileLocation;
                if (filePath == null)
                {
                    return NotFound(); // or handle the case when file doesn't exist
                }

                // Set the content type based on the file extension
                var contentType = GetContentType(filePath);

                // Return the file as a response
                var fileStream = new FileStream(filePath, FileMode.Open, FileAccess.Read);
                return File(fileStream, contentType, Path.GetFileName(filePath));

            }
            else
            {
                return BadRequest(response.StatusCode);
            }
            // Retrieve the file path based on the bookId
            
        }


        private string GetContentType(string filePath)
        {
            var provider = new FileExtensionContentTypeProvider();
            if (!provider.TryGetContentType(filePath, out var contentType))
            {
                contentType = "application/octet-stream"; // Default content type if mapping not found
            }
            return contentType;
        }
    }
}
