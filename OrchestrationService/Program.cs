using Microsoft.Extensions.DependencyInjection.Extensions;
using Newtonsoft.Json;
using OrchestrationService;
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

app.MapPost("/invokeservice/{id}", async (int id, IOrchestratorService orchestratorService, HttpClient client) =>
{
    var res = await orchestratorService.SearchInService((ServiceEnum)id);
    if (res?.Count() > 0)
    {
        var jsonBody = JsonConvert.SerializeObject(res);
        var body = new StringContent(jsonBody, Encoding.UTF8, "application/json");
        var response = await client.PostAsync("https://localhost:7231/processing/addbooks", body);
        if (response.IsSuccessStatusCode)
        {
            return Results.Ok(response);
        }
        else
        {
            return Results.BadRequest(response.StatusCode);
        }
    }
    return Results.NoContent();

})
.WithName("InvokeService");


app.Run();