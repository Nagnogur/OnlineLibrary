using OnlineLibrary.ApiParsers;
using OnlineLibrary.Models;

namespace ParsingService.Services
{
    public interface IScrapService
    {
        //public ApiModel GetApiModel();
        public IPageScrapper GetPageScrapper();
    }
}
