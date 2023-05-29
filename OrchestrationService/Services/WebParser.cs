using Microsoft.AspNetCore.WebUtilities;
using ParsingService.Models.Entities;

namespace OrchestrationService.Services
{
    public abstract class WebParser
    {
        public async Task<List<BookModel>> ParseWholeDomain(string searchUrl, KeyValuePair<string, int>? pagingQuery = null, int pageParseNumber = 1, int pageIncrement = 1)
        {
            List<BookModel> books = new List<BookModel>();

            if (pagingQuery != null)
            {
                List<string> links = new List<string>();

                for (int i = 0; i < pageParseNumber; i++)
                {
                    var urlWithPaging = QueryHelpers.AddQueryString(searchUrl, pagingQuery.Value.Key, pagingQuery.Value.Value.ToString());
                    pagingQuery = new KeyValuePair<string, int>(pagingQuery.Value.Key, pagingQuery.Value.Value + pageIncrement);

                    links.Add(urlWithPaging);
                }

                await Parallel.ForEachAsync(links, async (link, CancellationToken) =>
                {
                    try
                    {
                        var result = await ParsePage(link);
                        if (result != null)
                        {
                            lock (books)
                            {
                                books.AddRange(result);
                            }
                        }
                    }
                    catch 
                    {
                        return;
                    }
                    
                });
            }
            else
            {
                return await ParsePage(searchUrl);
            }

            return books;
        }
        public abstract Task<List<BookModel>> ParsePage(string url);

        public async Task<string> CallUrl(string fullUrl)
        {
            HttpClient client = new HttpClient();
            var response = await client.GetStringAsync(fullUrl);
            return response;
        }
    }
}
