using Microsoft.AspNetCore.Mvc;
using RecommendationService.Test;
using RecommendationSystem;

namespace RecommendationService.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class WeatherForecastController : ControllerBase
    {
        private static readonly string[] Summaries = new[]
        {
        "Freezing", "Bracing", "Chilly", "Cool", "Mild", "Warm", "Balmy", "Hot", "Sweltering", "Scorching"
    };

        private readonly ILogger<WeatherForecastController> _logger;

        public WeatherForecastController(ILogger<WeatherForecastController> logger)
        {
            _logger = logger;
        }

        [HttpGet(Name = "GetWeatherForecast")]
        public IEnumerable<WeatherForecast> Get()
        {
            BookRecommenderTest bookRecommender = new BookRecommenderTest();
            var data = bookRecommender.InitializeData();

            bookRecommender = new BookRecommenderTest(25, 50, 0.2f, data);
            Console.WriteLine("\n");

            bookRecommender = new BookRecommenderTest(40, 50, 0.2f, data);
            Console.WriteLine("\n");

            bookRecommender = new BookRecommenderTest(25, 100, 0.2f, data);
            Console.WriteLine("\n");

            bookRecommender = new BookRecommenderTest(40, 100, 0.2f, data);
            Console.WriteLine("\n");

            return null;
        }
    }
}