using Common.Extensions;
using Common.Options;
using FluentValidation;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using OpenTelemetry.Logs;
using OpenTelemetry.Resources;
using TopicService.Application.DTOs;
using TopicService.Infrastructure;
using TopicService.Infrastructure.Persistence;
using TopicService.Presentation.Extensions;
using TopicService.Presentation.Options;

var builder = WebApplication.CreateBuilder(args);

builder.Services
    .AddValidatorsFromAssemblyContaining<KeycloakOptions>(ServiceLifetime.Singleton)
    .AddValidatorsFromAssemblyContaining<Program>(ServiceLifetime.Singleton)
    .AddValidatorsFromAssemblyContaining<CreateTopicRequestValidator>(ServiceLifetime.Singleton)
    .RegisterOptions<TopicServiceOptions>(builder.Configuration)
    .RegisterAuthenticationSchemes(builder.Configuration)
    .RegisterPooledDbContextFactory<ApplicationDbContext, TopicServiceOptions>(Constants.DatabaseSchema)
    ;

builder.Services.Configure<ApiBehaviorOptions>(options => options.SuppressInferBindingSourcesForParameters = true);

builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowLocalhost",
        b => b.WithOrigins("https://localhost:4173") // Разрешите доступ только с этого домена
            .AllowAnyMethod() // Разрешите любые методы
            .AllowAnyHeader()); // Разрешите любые заголовки
});

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

public partial class Program
{
}