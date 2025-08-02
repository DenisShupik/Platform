using FileService.Infrastructure;
using FileService.Presentation;
using FileService.Presentation.Extensions;
using SharedKernel.Presentation.Extensions;

var builder = WebApplication.CreateBuilder(args);

builder.AddInfrastructureServices();
builder.AddPresentationServices();

var app = builder.Build();

app
    .UseExceptionHandler()
    .UseAuthentication()
    .UseAuthorization();

app
    .UseSwagger()
    .UseSwaggerUI();

app.MapApi();

app.Logger.StartingApp();

await app.RunAsync();