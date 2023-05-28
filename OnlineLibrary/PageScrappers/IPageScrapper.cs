using ParsingService.Models.Entities;

namespace OnlineLibrary.ApiParsers
{
    public interface IPageScrapper
    {
        public Task<IList<BookModel>> ParseWholeSite(string searchUrl, string baseUrl, KeyValuePair<string, int>? pagingQuery = null, int pageParseNumber = 1, int pageIncrement = 1);
        public Task<IList<BookModel>> ParsePage(string url, string baseUrl);//string html);
    }
}
