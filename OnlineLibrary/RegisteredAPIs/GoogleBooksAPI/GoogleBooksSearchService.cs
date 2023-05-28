using Microsoft.AspNetCore.WebUtilities;
using OnlineLibrary.ApiParsers;
using OnlineLibrary.Models;
using ParsingService.Services;

namespace ParsingService.RegisteredAPIs.GoogleBooksAPI
{
    public class GoogleBooksSearchService : ISearchService
    {
        public ApiModel GetApiModel()
        {
            return new GoogleApiModel();
        }

        public IApiParser GetApiParser()
        {
            return new GoogleBooksApiParser();
        }
    }
}
