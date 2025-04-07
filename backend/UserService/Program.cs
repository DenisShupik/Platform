using SharedKernel.Extensions;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using SharedKernel.Options;
using UserService.Application;
using UserService.Infrastructure;
using UserService.Infrastructure.Persistence;
using UserService.Presentation;
using UserService.Presentation.Apis.Rest;
using UserService.Presentation.Options;

var builder = WebApplication.CreateBuilder(args);

builder.AddApplicationServices();
builder.AddInfrastructureServices<UserServiceOptions>();
builder.AddPresentationServices();

builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowLocalhost",
        b => b.WithOrigins("https://localhost:8000","https://localhost:4173","https://localhost:5173")
            .AllowAnyMethod()
            .AllowAnyHeader());
});

var app = builder.Build();

using (var scope = app.Services.CreateScope())
{
    var factory = scope.ServiceProvider.GetRequiredService<IDbContextFactory<ApplicationDbContext>>();
    await using var dbContext = factory.CreateDbContext();
    await dbContext.Database.MigrateAsync();
}

app.UseCors("AllowLocalhost");

app
    .UseExceptionHandler()
    .UseAuthentication()
    .UseAuthorization()
    ;

app.MapUserApi();

if (app.Environment.IsDevelopment())
{
    app
        .UseSwagger()
        .UseSwaggerUI(options =>
        {
            var keycloakOptions = app.Services.GetRequiredService<IOptions<KeycloakOptions>>().Value;
            options.OAuthClientId(keycloakOptions.Audience);
            options.OAuthUsePkce();
        });
}

app.Logger.StartingApp();

await app.RunAsync();