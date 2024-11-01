using System.Reflection;
using Common.Extensions;
using FluentValidation;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using UserService.Infrastructure.Persistence;
using UserService.Presentation.Extensions;
using UserService.Presentation.Options;

var builder = WebApplication.CreateBuilder(args);

builder.Services
    .AddValidatorsFromAssembly(Assembly.Load(nameof(Common)), ServiceLifetime.Singleton)
    .AddValidatorsFromAssemblyContaining<Program>(ServiceLifetime.Singleton)
    .RegisterOptions<UserServiceOptions>(builder.Configuration)
    .RegisterAuthenticationSchemes(builder.Configuration)
    .RegisterPooledDbContextFactory<ApplicationDbContext, UserServiceOptions>(UserService.Infrastructure.Constants.DatabaseSchema)
    .RegisterEventBus(builder.Configuration)
    ;

builder.Services.Configure<ApiBehaviorOptions>(options => options.SuppressInferBindingSourcesForParameters = true);

builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowLocalhost",
        b => b.WithOrigins("https://localhost:4173","https://localhost:5173")
            .AllowAnyMethod()
            .AllowAnyHeader());
});

builder.Services.RegisterSwaggerGen();

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