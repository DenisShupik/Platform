using ApiGateway.Infrastructure;
using ApiGateway.Presentation;
using ApiGateway.Presentation.Rest;
using Microsoft.Extensions.Options;
using Shared.Infrastructure.Options;

var builder = WebApplication.CreateBuilder(args);

builder.AddInfrastructureServices();
builder.AddPresentationServices();

var app = builder.Build();

if (app.Environment.IsDevelopment()) app.UseCors("AllowLocalhost");

app.MapReverseProxy();
app.MapApi();

app.UseSwaggerUI(options =>
{
    options.SwaggerEndpoint("/api/openapi.json", "API Gateway");
    var keycloakOptions = app.Services.GetRequiredService<IOptions<KeycloakOptions>>().Value;
    // TODO: не работает, фикса пока нет
    options.OAuthClientId(keycloakOptions.Audience);
    options.OAuthUsePkce();
});

await app.RunAsync();