using Microsoft.AspNetCore.WebUtilities;
using Newtonsoft.Json;
using ParsingService.Models.Entities;

namespace OrchestrationService.Services
{
    public abstract class ApiParser
    {
        public async Task<IEnumerable<BookModel>?> ParseAllCalls(HttpClient client, string searchUrl,
            KeyValuePair<string, int>? pagingQuery = null, int pageParseNumber = 1, int pageIncrement = 1,
            Dictionary<string, string>? staticQueryParameters = null, List<(string, string)>? changingValueParameters = null)
        {
            List<BookModel> serviceSearchResult = new List<BookModel>();

            List<string> urls = new List<string>();

            if (searchUrl == null)
            {
                return null;
            }
            string url = searchUrl;
            if (staticQueryParameters != null)
            {
                foreach (var parameter in staticQueryParameters)
                {
                    url = QueryHelpers.AddQueryString(url, parameter.Key, parameter.Value);
                }
            }
            if (changingValueParameters != null)
            {
                foreach (var parameter in changingValueParameters)
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
                int receivedItems = 0;
                var currentPagingParameters = pagingQuery;
                for (int i = 0; i < pageParseNumber; i++)
                {
                    var urlWithPaging = urlWithQueries;
                    if (currentPagingParameters != null)
                    {
                        urlWithPaging = QueryHelpers.AddQueryString(urlWithQueries, currentPagingParameters.Value.Key, currentPagingParameters.Value.Value.ToString());
                        if (pageIncrement != null)
                        {
                            currentPagingParameters = new KeyValuePair<string, int>(currentPagingParameters.Value.Key, currentPagingParameters.Value.Value + pageIncrement);
                        }
                    }

                    HttpResponseMessage response = await client.GetAsync(urlWithPaging);
                    if (response.IsSuccessStatusCode)
                    {
                        var deserializedResponse = JsonConvert.DeserializeObject<dynamic>(await response.Content.ReadAsStringAsync());
                        List<BookModel> parsedResponse;
                        try
                        {
                            parsedResponse = ParseResponse(deserializedResponse);
                        }
                        catch (Exception ex)
                        {
                            break;
                        }

                        if (parsedResponse != null && parsedResponse.Count > 0)
                        {
                            receivedItems = parsedResponse.Count;
                            serviceSearchResult.AddRange(parsedResponse);
                        }
                        else
                        {
                            break;
                        }
                    }
                    else break;
                }
            }
            return serviceSearchResult;
        }


        public abstract IList<BookModel> ParseResponse(dynamic response);
    }
}
