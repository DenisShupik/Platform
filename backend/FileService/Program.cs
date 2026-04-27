using FileService.Infrastructure;
using FileService.Presentation;
using FileService.Presentation.Extensions;
using FileService.Presentation.Rest;
using Shared.Presentation.Extensions;
using static FileService.Infrastructure.Constants;

var builder = WebApplication.CreateBuilder(args);

builder.AddInfrastructureServices();
builder.AddPresentationServices();

var app = builder.Build();

await app.EnsureS3Init(AvatarBucket);

app
    .UseExceptionHandler()
    .UseAuthentication()
    .UseAuthorization();

app.MapOpenApi("/api/{documentName}.json");

app.MapApi();

app.Logger.StartingApp();

await app.RunAsync();