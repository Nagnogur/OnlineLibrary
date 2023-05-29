using ParsingService.Models;
using ParsingService.Models.Entities;

namespace OrchestrationService.Services
{
    public interface ISearchService
    {
        public Task<List<BookModel>> GetFromService();
        public QueryModel GetQueryModel();
    }
}
