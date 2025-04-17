using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using SharedKernel.Presentation.Extensions;
using SharedKernel.Presentation.Options;
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

var app = builder.Build();

using (var scope = app.Services.CreateScope())
{
    var factory = scope.ServiceProvider.GetRequiredService<IDbContextFactory<ApplicationDbContext>>();
    await using var dbContext = factory.CreateDbContext();
    await dbContext.Database.MigrateAsync();
}

app
    .UseExceptionHandler()
    .UseAuthentication()
    .UseAuthorization()
    ;

app.MapUserApi();

app
    .UseSwagger()
    .UseSwaggerUI(options =>
    {
        var keycloakOptions = app.Services.GetRequiredService<IOptions<KeycloakOptions>>().Value;
        options.OAuthClientId(keycloakOptions.Audience);
        options.OAuthUsePkce();
    });

app.Logger.StartingApp();

await app.RunAsync();