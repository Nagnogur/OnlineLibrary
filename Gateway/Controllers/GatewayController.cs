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

// For more information on enabling Web API for empty projects, visit https://go.microsoft.com/fwlink/?LinkID=397860

namespace Gateway.Controllers
{
    [Route("api")]
    [ApiController]
    public class GatewayController : ControllerBase
    {
        private HttpClient client;
        private IMapper mapper;

        string orchestratorUrl = "https://localhost:7021/invokeservice/";
        MapperConfiguration configuration = new MapperConfiguration(cfg => {
            cfg.AddProfile<MapperProfile>();
        });

        public GatewayController(HttpClient client)
        {
            this.client = client;
            mapper = configuration.CreateMapper();
        }

        // GET: api/<GatewayController>
        [HttpPost("{id}")]
        public async Task<IActionResult> InvokeService(int id)
        {
            string url = orchestratorUrl + id;
            var response = await client.PostAsync(url, null);
            if (response.IsSuccessStatusCode)
            {
                return Ok();
            }
            else
            {
                return BadRequest();
            }

            //var res = await orchestratorService.SearchInService((ServiceEnum)id);

            /*var jsonBody = JsonConvert.SerializeObject(res);
            var body = new StringContent(jsonBody, Encoding.UTF8, "application/json");
            var response = await client.PostAsync(processingServerUrl, body);
            if (response.IsSuccessStatusCode)
            {
                return Results.Ok(response);
            }
            else
            {
                return Results.BadRequest(response.StatusCode);
            }


            return null;*/
        }

        public enum ServiceEnum
        {
            GoogleBooks,
            RoyalRoad,
        }

        // GET api/<GatewayController>
        [HttpGet]
        public async Task<IActionResult> Get([FromQuery] BookQueryParameters query)
        {
            string sourceUrl = "https://localhost:7248/api/books";
            var fullUrl = sourceUrl + Request.QueryString.Value;

            var response = await client.GetAsync(fullUrl);
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

        //POST api/<GatewayController>
        [HttpPost("addbook")]
        public async Task<IActionResult> AddBook([FromBody] BookModel book)
        {
            string processingServerUrl = "https://localhost:7248/api/addbooks";

            Book newBook = mapper.Map<Book>(book);
            List<Book> request = new List<Book>() { newBook };

            var jsonBody = JsonConvert.SerializeObject(request);
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

        //public record PeriodicScanServiceState(bool IsEnabled);

        [HttpPatch("togglescan")]
        public async Task<bool> ToggleScanService(bool enable)
        {
            string toggleUrl = "https://localhost:7021/scanservicetoggle";
            string scanValue = "https://localhost:7021/scanservicestate";

            toggleUrl = QueryHelpers.AddQueryString(toggleUrl, "state", enable.ToString());

            /*var jsonBody = JsonConvert.SerializeObject(enable);
            var body = new StringContent(jsonBody, Encoding.UTF8, "application/json");*/
            var patchResponse = await client.PatchAsync(toggleUrl, null);

            if (patchResponse.IsSuccessStatusCode)
            {
                var scanValueResponce = await client.GetAsync(scanValue);
                if (scanValueResponce.IsSuccessStatusCode)
                {
                    var res = JsonConvert.DeserializeObject<bool>(await scanValueResponce.Content.ReadAsStringAsync());
                    return res;
                }
            }
            return false;
        }



        /*// POST api/<GatewayController>
        [HttpPost]
        public void Post([FromBody] string value)
        {
        }

        // PUT api/<GatewayController>/5
        [HttpPut("{id}")]
        public void Put(int id, [FromBody] string value)
        {
        }

        // DELETE api/<GatewayController>/5
        [HttpDelete("{id}")]
        public void Delete(int id)
        {
        }*/
    }
}
