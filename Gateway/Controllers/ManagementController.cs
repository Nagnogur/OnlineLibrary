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
using System.Text;

namespace Gateway.Controllers
{
    [Authorize(Roles = "Admin")]
    [Route("management")]
    [ApiController]
    public class ManagementController : ControllerBase
    {
        private HttpClient client;
        private IHttpClientFactory clientFactory;
        private IMapper mapper;

        MapperConfiguration mapperConfiguration = new MapperConfiguration(cfg => {
            cfg.AddProfile<MapperProfile>();
        });
        public ManagementController(HttpClient client, IHttpClientFactory clientFactory)
        {
            this.client = client;
            mapper = mapperConfiguration.CreateMapper();
            this.clientFactory = clientFactory;
        }

        // GET: api/<GatewayController>
        [HttpPost]
        [Route("scan")]
        public async Task<IActionResult> InvokeService(ServiceEnum service)
        {
            var id = (int)service;
            //string url = orchestratorUrl + id;
            var invokeClient = clientFactory.CreateClient("InvokeServiceClient");
            var response = await invokeClient.PostAsync(invokeClient.BaseAddress + id.ToString(), null);
            if (response.IsSuccessStatusCode)
            {
                return Ok();
            }
            else
            {
                return BadRequest();
            }
        }
        
        [HttpPost]
        [Route("settimer")]
        public async Task<IActionResult> SetScanTimer(double minutes)
        {
            var url = "https://localhost:7021/setscantimer";
            url = QueryHelpers.AddQueryString(url, "minutes", minutes.ToString());

            var response = await client.PostAsync(url, null);
            if (response.IsSuccessStatusCode)
            {
                return Ok();
            }
            else
            {
                return BadRequest();
            }
        }

        public enum ServiceEnum
        {
            GoogleBooks,
            RoyalRoad,
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

        [HttpPost("forceadd")]
        public async Task<IActionResult> ForceAddBook([FromBody] BookModel book)
        {
            string processingServerUrl = "https://localhost:7248/api/forceaddbook";

            Book newBook = mapper.Map<Book>(book);

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

        [HttpPut("forceupdate")]
        public async Task<IActionResult> ForceUpdate([FromBody] BookModel book, string? newTitle = null)
        {
            string processingServerUrl = "https://localhost:7248/api/forceupdate";

            if (newTitle != null)
            {
                processingServerUrl = QueryHelpers.AddQueryString(processingServerUrl, "title", newTitle);
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

        [HttpDelete("delete")]
        public async Task<IActionResult> Delete(string title)
        {
            string processingServerUrl = "https://localhost:7248/api/deleteByTitle";

            processingServerUrl = QueryHelpers.AddQueryString(processingServerUrl, "title", title);

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

        [HttpGet("retrain")]
        public async Task<IActionResult> RetrainModel()
        {
            var url = "https://localhost:7248/api/ratings";

            var response = await client.GetAsync(url);
            if (response.IsSuccessStatusCode)
            {
                var res = JsonConvert.DeserializeObject<List<Review>>(await response.Content.ReadAsStringAsync());

                var recommenderUrl = "https://localhost:7275/recommender";
                var recommednations = await client.PostAsync(recommenderUrl, response.Content);
                return Ok(res);
            }
            else
            {
                return BadRequest(response.StatusCode);
            }
        }
    }
}
