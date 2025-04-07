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
using CoreService.Presentation.Options;
using SharedKernel.Infrastructure.Extensions.ServiceCollectionExtensions;
using SharedKernel.Presentation.Extensions.ServiceCollectionExtensions;
using ZiggyCreatures.Caching.Fusion.Backplane.StackExchangeRedis;

var builder = WebApplication.CreateBuilder(args);

builder.Services
    .AddValidatorsFromAssemblyContaining<KeycloakOptions>(ServiceLifetime.Singleton)
    .AddValidatorsFromAssemblyContaining<Program>(ServiceLifetime.Singleton)
    .AddValidatorsFromAssemblyContaining<CreateThreadRequestValidator>(ServiceLifetime.Singleton)
    .RegisterOptions<CoreServiceOptions>(builder.Configuration)
    .RegisterAuthenticationSchemes(builder.Configuration)
    .RegisterPooledDbContextFactory<ApplicationDbContext, CoreServiceOptions>(Constants.DatabaseSchema)
    ;

builder.Services.Configure<ApiBehaviorOptions>(options => options.SuppressInferBindingSourcesForParameters = true);

builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowLocalhost",
        b => b.WithOrigins("https://localhost:8000","http://localhost:4173","http://localhost:5173")
            .AllowAnyMethod()
            .AllowAnyHeader());
});

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

builder.Services.RegisterSwaggerGen();

builder.Services.AddOpenTelemetry()
    .ConfigureResource(r => r.AddService(builder.Environment.ApplicationName))
    .WithLogging(logging => logging.AddOtlpExporter());

builder.WebHost.UseKestrelHttpsConfiguration();

var app = builder.Build();

using (var scope = app.Services.CreateScope())
{
    var factory = scope.ServiceProvider.GetRequiredService<IDbContextFactory<ApplicationDbContext>>();
    await using var dbContext = factory.CreateDbContext();
    await dbContext.Database.MigrateAsync();
}

app.UseCors("AllowLocalhost");

app
    .UseSwagger()
    .UseSwaggerUI();

app
    .UseAuthentication()
    .UseAuthorization();

app.MapApi();

app.Logger.StartingApp();

await app.RunAsync();