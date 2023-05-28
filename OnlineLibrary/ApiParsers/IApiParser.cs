using ParsingService.Models.Entities;

namespace OnlineLibrary.ApiParsers
{
    public interface IApiParser
    {
        public IList<BookModel> ParseResponse(dynamic response);
    }
}
