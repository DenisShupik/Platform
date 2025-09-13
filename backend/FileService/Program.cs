using FileService.Infrastructure;
using FileService.Presentation;
using FileService.Presentation.Rest;
using Shared.Presentation.Extensions;

var builder = WebApplication.CreateBuilder(args);

builder.AddInfrastructureServices();
builder.AddPresentationServices();

var app = builder.Build();

app
    .UseExceptionHandler()
    .UseAuthentication()
    .UseAuthorization();

app.MapOpenApi("/api/{documentName}.json");

app.MapApi();

app.Logger.StartingApp();

await app.RunAsync();