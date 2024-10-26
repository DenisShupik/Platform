using Common.Extensions;
using Common.Options;
using FluentValidation;
using Microsoft.EntityFrameworkCore;
using NoteService.Application.DTOs;
using NoteService.Infrastructure.Persistence;
using NoteService.Presentation.Apis;
using NoteService.Presentation.Extensions;
using NoteService.Presentation.Options;
using OpenTelemetry.Logs;
using OpenTelemetry.Resources;

var builder = WebApplication.CreateBuilder(args);

builder.Services
    .AddValidatorsFromAssemblyContaining<KeycloakOptions>(ServiceLifetime.Singleton)
    .AddValidatorsFromAssemblyContaining<Program>(ServiceLifetime.Singleton)
    .AddValidatorsFromAssemblyContaining<GetNotesByUserIdRequestValidator>(ServiceLifetime.Singleton)
    .RegisterOptions<NoteServiceOptions>(builder.Configuration)
    .RegisterAuthenticationSchemes(builder.Configuration)
    .RegisterPooledDbContextFactory<ApplicationDbContext, NoteServiceOptions>("note_service")
    ;

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

app
    .UseAuthentication()
    .UseAuthorization();

app
    .UseSwagger()
    .UseSwaggerUI();

app.MapNoteApi();

app.Logger.StartingApp();

await app.RunAsync();

public partial class Program
{
}