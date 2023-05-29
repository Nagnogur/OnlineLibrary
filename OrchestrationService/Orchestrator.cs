using OrchestrationService.RegisteredServices;
using OrchestrationService.RegisteredServices.GoogleBooksAPIService;
using OrchestrationService.Services;
using ParsingService.Models.Entities;

namespace OrchestrationService
{
    public class Orchestrator : IOrchestratorService
    {
        public async Task<List<BookModel>> SearchInService(ServiceEnum serviceName)
        {
            ISearchService searchService = GetSearchService(serviceName);

            var res = await searchService.GetFromService();

            return res;
        }

        public ISearchService GetSearchService(ServiceEnum service)
        {
            switch (service)
            {
                case ServiceEnum.GoogleBooks:
                    {
                        return new GoogleBooksApiParser();
                    }
                case ServiceEnum.RoyalRoad:
                    {
                        return new RoyalRoadScrapper();
                    }
                default:
                    {
                        throw new NotImplementedException();
                    }
            }
        }
    }

    public interface IOrchestratorService
    {
        Task<List<BookModel>> SearchInService(ServiceEnum serviceName);
    }

    public enum ServiceEnum
    {
        GoogleBooks,
        RoyalRoad,
        Another,
    }
}
