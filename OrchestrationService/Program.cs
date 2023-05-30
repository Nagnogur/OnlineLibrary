using Microsoft.Extensions.DependencyInjection.Extensions;
using Newtonsoft.Json;
using OrchestrationService;
using OrchestrationService.Services;
using System.Text;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();
builder.Services.AddHttpClient();
builder.Services.TryAddScoped<IOrchestratorService, Orchestrator>();
builder.Services.AddScoped<ScanService>();
builder.Services.AddSingleton<PeriodicScanService>();
builder.Services.AddHostedService(provider => provider.GetRequiredService<PeriodicScanService>());

builder.Services.AddCors(p => p.AddPolicy("DeleteLater", builder =>
{
    builder.WithOrigins("*").AllowAnyMethod().AllowAnyHeader();
}));


var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();

//string processingServerUrl = "https://localhost:7248/api/addbooks";

app.MapPost("/invokeservice/{id}", async (int id, IOrchestratorService orchestratorService) =>
{
    /*var res = await orchestratorService.SearchInService((ServiceEnum)id);
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
    return Results.NoContent();*/

    return await orchestratorService.SaveToDatabase(id);

})
.WithName("InvokeService");


app.MapGet("/scanservicestate", (PeriodicScanService service) =>
{
    return service.IsEnabled;
});

app.MapMethods("/scanservicetoggle", new[] { "PATCH" }, (
    bool state,
    PeriodicScanService service) =>
{
    service.IsEnabled = state;
});

app.MapPost("/setscantimer", (double minutes, PeriodicScanService scanService) =>
{
    scanService.Period = TimeSpan.FromMinutes(minutes);
    return scanService.Period;
});

app.Run();