using Newtonsoft.Json;
using OrchestrationService.RegisteredServices;
using OrchestrationService.RegisteredServices.GoogleBooksAPIService;
using OrchestrationService.Services;
using ParsingService.Models.Entities;
using System.Text;

namespace OrchestrationService
{
    public class Orchestrator : IOrchestratorService
    {
        private readonly ILogger<Orchestrator> _logger;
        private readonly HttpClient client;
        private readonly string processingServerUrl = "https://localhost:7248/api/addbooks";

        public Orchestrator(ILogger<Orchestrator> logger, HttpClient client)
        {
            _logger = logger;
            this.client = client;
        }
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

        public async Task<IResult> SaveToDatabase(int id)
        {
            var res = await SearchInService((ServiceEnum)id);
            if (res?.Count() > 0)
            {
                var jsonBody = JsonConvert.SerializeObject(res);
                var body = new StringContent(jsonBody, Encoding.UTF8, "application/json");
                var response = await client.PostAsync(processingServerUrl, body);
                if (response.IsSuccessStatusCode)
                {
                    return Results.Ok();
                }
                else
                {
                    return Results.BadRequest();
                }
            }
            return Results.NoContent();
        }

    }

    public class ScanService
    {
        private readonly ILogger<Orchestrator> _logger;
        private readonly IOrchestratorService orchestrator;

        public ScanService(ILogger<Orchestrator> logger, IOrchestratorService orchestrator)
        {
            _logger = logger;
            this.orchestrator = orchestrator;
        }
        public async Task InvokeServices()
        {
            foreach (int service in Enum.GetValues(typeof(ServiceEnum)))//ConsoleColor)))
            {
                var res = await orchestrator.SaveToDatabase(service);
                if (res == Results.Ok())
                {
                    _logger.LogInformation("Successfully scanned {0} service.", (ServiceEnum)service);
                }
                else if (res == Results.BadRequest())
                {
                    _logger.LogInformation("Scan failed for {0} service.", (ServiceEnum)service);
                }
                else
                {
                    _logger.LogInformation("Scanned {0} service but nothing found.", (ServiceEnum)service);
                }
            }
        }
    }

    public interface IOrchestratorService
    {
        Task<List<BookModel>> SearchInService(ServiceEnum serviceName);
        Task<IResult> SaveToDatabase(int id);
    }

    public enum ServiceEnum
    {
        GoogleBooks,
        RoyalRoad
    }
}
