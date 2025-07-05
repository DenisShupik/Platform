using FluentValidation;
using Microsoft.EntityFrameworkCore;
using NotificationService.Application;
using NotificationService.Infrastructure;
using NotificationService.Infrastructure.Persistence;
using NotificationService.Presentation.Extensions;
using NotificationService.Presentation.Filters;
using OpenTelemetry.Logs;
using OpenTelemetry.Resources; 
using NotificationService.Presentation.Options;
using SharedKernel.Presentation.Extensions;
using SharedKernel.Presentation.Extensions.ServiceCollectionExtensions;  

var builder = WebApplication.CreateBuilder(args);

builder.Services
    .RegisterOptions<NotificationServiceOptions, NotificationServiceOptionsValidator>(builder.Configuration)
    .RegisterAuthenticationSchemes(builder.Configuration)
    ;
builder.Services.RegisterSwaggerGen(options => { options.DocumentFilter<TypesDocumentFilter>(); });

builder.Services.AddOpenTelemetry()
    .ConfigureResource(r => r.AddService(builder.Environment.ApplicationName))
    .WithLogging(logging => logging.AddOtlpExporter());

builder.AddApplicationServices();
builder.AddInfrastructureServices<NotificationServiceOptions>();

var app = builder.Build();

using (var scope = app.Services.CreateScope())
{
    var factory = scope.ServiceProvider.GetRequiredService<IDbContextFactory<ApplicationDbContext>>();
    await using var dbContext = factory.CreateDbContext();
    await dbContext.Database.MigrateAsync();
}

app
    .UseSwagger()
    .UseSwaggerUI();

app
    .UseAuthentication()
    .UseAuthorization();

app.MapApi();

app.Logger.StartingApp();

await app.RunAsync();

public sealed partial class Program;