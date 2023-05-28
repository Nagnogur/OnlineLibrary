using ParsingService.Models.Entities;

namespace ParsingService.Orchestration.SearchService
{
    public interface IOrchestratorService
    {
        Task<IEnumerable<BookModel>?> SearchInService(ServiceEnum serviceName);
    }
}
