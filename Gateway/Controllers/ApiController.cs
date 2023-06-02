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
    }
}
