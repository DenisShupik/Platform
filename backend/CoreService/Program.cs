using CoreService.Application;
using FluentValidation;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.StackExchangeRedis;
using Microsoft.Extensions.Options;
using OpenTelemetry.Logs;
using OpenTelemetry.Resources;
using SharedKernel.Extensions;
using SharedKernel.Options;
using CoreService.Application.UseCases;
using CoreService.Infrastructure;
using CoreService.Infrastructure.Persistence;
using CoreService.Presentation.Extensions;
using CoreService.Presentation.Filters;
using CoreService.Presentation.Options;
using SharedKernel.Presentation.Extensions.ServiceCollectionExtensions;
using ZiggyCreatures.Caching.Fusion.Backplane.StackExchangeRedis;

var builder = WebApplication.CreateBuilder(args);

builder.Services
    .AddValidatorsFromAssemblyContaining<Program>(ServiceLifetime.Singleton)
    .AddValidatorsFromAssemblyContaining<CreateThreadRequestValidator>(ServiceLifetime.Singleton)
    .RegisterOptions<CoreServiceOptions, CoreServiceOptionsValidator>(builder.Configuration)
    .RegisterAuthenticationSchemes(builder.Configuration)
    ;

builder.Services.Configure<ApiBehaviorOptions>(options => options.SuppressInferBindingSourcesForParameters = true);

builder.Services.AddFusionCacheSystemTextJsonSerializer();
builder.Services
    .AddOptions<RedisBackplaneOptions>()
    .Configure<IOptions<RedisOptions>>((options, redisOptions) =>
    {
        options.Configuration = redisOptions.Value.ConnectionString;
    });
builder.Services
    .AddOptions<RedisCacheOptions>()
    .Configure<IOptions<RedisOptions>>((options, redisOptions) =>
    {
        options.Configuration = redisOptions.Value.ConnectionString;
    });
builder.Services.AddStackExchangeRedisCache(_ => { });
builder.Services.AddFusionCacheStackExchangeRedisBackplane();

builder.Services.RegisterSwaggerGen(options => { options.DocumentFilter<TypesDocumentFilter>(); });

builder.Services.AddOpenTelemetry()
    .ConfigureResource(r => r.AddService(builder.Environment.ApplicationName))
    .WithLogging(logging => logging.AddOtlpExporter());

builder.AddApplicationServices();
builder.AddInfrastructureServices<CoreServiceOptions>();

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