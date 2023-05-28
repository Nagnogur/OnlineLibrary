using Microsoft.AspNetCore.WebUtilities;
using Newtonsoft.Json;
using OnlineLibrary.ApiParsers;
using OnlineLibrary.Models;
using ParsingService.Models.Entities;
using ParsingService.RegisteredAPIs.GoogleBooksAPI;
using ParsingService.Services;

namespace ParsingService.Orchestration.SearchService
{
    public class Orchestrator : IOrchestratorService
    {
        static HttpClient client = new HttpClient();

        //private string path = "https://www.googleapis.com/books/v1/volumes?q=subject:fiction&maxResults=20&startIndex=0";

        public async Task<IEnumerable<BookModel>?> SearchInService(ServiceEnum serviceName)
        {
            List<BookModel> serviceSearchResult = new List<BookModel>();

            ISearchService searchService = GetSearchService(serviceName);
            ApiModel apiModel = searchService.GetApiModel();
            IApiParser apiParser = searchService.GetApiParser();
            List<string> urls = new List<string>();

            if (apiModel.path == null)
            {
                return null;
            }
            string url = apiModel.path;
            if (apiModel.constantQueryParameters != null)
            {
                foreach (var parameter in apiModel.constantQueryParameters)
                {
                    url = QueryHelpers.AddQueryString(url, parameter.Key, parameter.Value);
                }
            }
            if (apiModel.changingValueParameters != null)
            {
                foreach (var parameter in apiModel.changingValueParameters)
                {
                    var changingUrl = QueryHelpers.AddQueryString(url, parameter.Item1, parameter.Item2);
                    urls.Add(changingUrl);
                }
            }
            else
            {
                urls.Add(url);
            }

            foreach (var urlWithQueries in urls)
            {
                int receivedItems = 1;
                var currentPagingParameters = apiModel.pagingParameters;
                while (receivedItems > 0)
                {
                    var urlWithPaging = urlWithQueries;
                    if (currentPagingParameters != null)
                    {
                        urlWithPaging = QueryHelpers.AddQueryString(urlWithQueries, currentPagingParameters.Value.Item1, currentPagingParameters.Value.Item2.ToString());
                        if (apiModel.itemsPerPage != null)
                        {
                            currentPagingParameters = ((string, int)?)(currentPagingParameters.Value.Item1, currentPagingParameters.Value.Item2 + apiModel.itemsPerPage);
                        }
                    }

                    HttpResponseMessage response = await client.GetAsync(urlWithPaging);
                    if (response.IsSuccessStatusCode)
                    {
                        var deserializedResponse = JsonConvert.DeserializeObject<dynamic>(await response.Content.ReadAsStringAsync());
                        List<BookModel> parsedResponse;
                        try
                        {
                            parsedResponse = apiParser.ParseResponse(deserializedResponse);
                        }
                        catch (Exception ex)
                        {
                            break;
                        }

                        if (parsedResponse != null)
                        {
                            receivedItems = parsedResponse.Count;
                            serviceSearchResult.AddRange(parsedResponse);
                            /*var res = await SendParsedItems(parsedResponse);
                            return Results.Ok(res);*/
                        }
                        else
                        {
                            break;
                        }
                    }
                    else break;
                    //return Results.NotFound(); ;
                }
            }
            return serviceSearchResult;
        }


        public ISearchService GetSearchService(ServiceEnum service)
        {
            switch (service)
            {
                case ServiceEnum.GoogleBooks:
                    {
                        return new GoogleBooksSearchService();
                    }
                case ServiceEnum.AnotherService:
                    {
                        throw new NotImplementedException();
                    }
                default:
                    {
                        throw new NotImplementedException();
                    }
            }
        }

    }
}
