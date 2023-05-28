using Microsoft.Extensions.DependencyInjection.Extensions;
using Newtonsoft.Json;
using OnlineLibrary.ApiParsers;
using OnlineLibrary.Models;
using ParsingService.Orchestration.SearchService;
using ParsingService.RegisteredPages.Royalroad;
using System;
using System.Text;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();
builder.Services.AddHttpClient();
builder.Services.TryAddScoped<IOrchestratorService, Orchestrator>();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();

var summaries = new[]
{
    "Freezing", "Bracing", "Chilly", "Cool", "Mild", "Warm", "Balmy", "Hot", "Sweltering", "Scorching"
};

/*app.MapPost("/test", (ExternalApiService apiService, ApiModel model) => apiService.CallExternalApi(model, new GoogleBooksParserCreator()))
.WithName("GetBooksGoogle");*/

app.MapPost("/searchinservice/{id}", async (int id, IOrchestratorService orchestratorService, HttpClient client) =>
{
    var res = await orchestratorService.SearchInService((ServiceEnum)id);
    if (res?.Count() > 0)
    {
        var jsonBody = JsonConvert.SerializeObject(res);
        var body = new StringContent(jsonBody, Encoding.UTF8, "application/json");
        var response = await client.PostAsync("https://localhost:7231/processing/addbooks", body);
        if (response.IsSuccessStatusCode)
        {
            //cheers
            return Results.Ok(response);
        }
        else
        {
            return Results.BadRequest(response.StatusCode);
        }
    }
    return Results.NoContent();
    
})
.WithName("SearchInService");

/*app.MapPost("/saveparseditems", async (HttpClient client, List<BookModel> items) =>
)
.WithName("SaveParsedItems");*/

app.MapPost("/scraproyalroad", async (HttpClient client) =>
{
    IPageScrapper Scrapper = new RoyalRoadScrapper();
    var response = await Scrapper.ParseWholeSite("https://www.royalroad.com/fictions/search", "https://www.royalroad.com",
        new KeyValuePair<string, int>("page", 1), pageParseNumber: 5);
    return Results.Ok(response);
})
.WithName("ScrapPage");

app.Run();

