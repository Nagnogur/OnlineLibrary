using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ApplicationModels;
using Microsoft.AspNetCore.WebUtilities;
using Newtonsoft.Json;
using OnlineLibrary.ApiParsers;
using OnlineLibrary.Models;
using System.IO;
using System.Net.Sockets;
using System.Runtime.Serialization;
using System.Text;
using System.Text.Json;

/*
 {
  "path": "https://www.googleapis.com/books/v1/volumes",
  "endpoints": [
    "string"
  ],
  "queryParameters": {
    "q": "subject:fiction",
    "maxResults": "20",
    "startIndex": "0"
  }
}
 */

namespace OnlineLibrary.Services
{
    /*public class ExternalApiService
    {
        static HttpClient client = new HttpClient();
        public async Task<IResult> CallExternalApi(ApiModel apiModel, ParserCreator apiParser)
        {
            if (apiModel == null || apiModel.path == null)
            {
                return Results.UnprocessableEntity();
            }
            var path = apiModel.path;
            string url;

            if (apiModel.queryParameters != null)
            {
                var query = apiModel.queryParameters;
                url = QueryHelpers.AddQueryString(path, query);
            }
            else
            {
                url = path;
            }
            
            //"https://www.googleapis.com/books/v1/volumes?q=subject:fiction&key=AIzaSyASWX-eU5jd_ijshohUy44GEnowTlrhNlI";
            
            
            HttpResponseMessage response = await client.GetAsync("https://www.googleapis.com/books/v1/volumes?q=subject:fiction&maxResults=20&startIndex=0");
            if (response.IsSuccessStatusCode)
            {
                var pr = JsonConvert.DeserializeObject<dynamic>(await response.Content.ReadAsStringAsync());
                List<BookModel> parsedRes;
                try
                {
                    parsedRes = apiParser.Parse(pr);
                }
                catch (Exception ex)
                {
                    return Results.Problem();
                }

                if (parsedRes != null)
                {
                    var res = await SendParsedItems(parsedRes);
                    return Results.Ok(res);
                }
                else
                {
                    return Results.NoContent();
                }
            }
            return Results.NotFound(); ;
        }

        public async Task<IResult> SendParsedItems(List<BookModel> items)
        {
            int unsuccessfulCount = 0;
            var data = JsonConvert.SerializeObject(items);
            var httpContent = new StringContent(data, Encoding.UTF8, "application/json");
            HttpResponseMessage response = await client.PostAsync("https://localhost:7231/processing/test", httpContent);
            if (!response.IsSuccessStatusCode)
            {
                unsuccessfulCount++;
            }
            if (unsuccessfulCount > 0)
            {
                return Results.Accepted(unsuccessfulCount.ToString() + " failures");
            }
            return Results.Ok();
        }

    }*/

    /*public class RequestSender
    {
        private static HttpClient client;

        public RequestSender(string uri)
        {
            if (client == null)
            {
                client = new HttpClient
                {
                    BaseAddress = new Uri(uri),
                    
                };
            }
        }

        public async Task<string> Get(string path)
        {
            var response = await _client.GetStringAsync(path);
            return response;
        }
    }*/
}
