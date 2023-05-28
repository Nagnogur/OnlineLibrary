using OnlineLibrary.ApiParsers;
using OnlineLibrary.Models;

namespace ParsingService.Services
{
    public interface ISearchService
    {
        public ApiModel GetApiModel();
        public IApiParser GetApiParser();
    }
}
