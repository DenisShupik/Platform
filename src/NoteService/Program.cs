using NoteService.Extensions;
using OpenTelemetry.Logs;
using OpenTelemetry.Resources;

var builder = WebApplication.CreateBuilder(args);
builder.Services.AddOpenTelemetry()
    .ConfigureResource(r => r.AddService(builder.Environment.ApplicationName))
    .WithLogging(logging => logging.AddOtlpExporter());

builder.WebHost.UseKestrelHttpsConfiguration();

var app = builder.Build();

app.Logger.StartingApp();

await app.RunAsync();