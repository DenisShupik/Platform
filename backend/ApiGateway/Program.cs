using ApiGateway.Infrastructure;
using ApiGateway.Presentation;
using ApiGateway.Presentation.Rest;
using Microsoft.Extensions.Options;
using Shared.Infrastructure.Options;
using Shared.Presentation.Extensions;

var builder = WebApplication.CreateBuilder(args);

builder.AddInfrastructureServices();
builder.AddPresentationServices();

var app = builder.Build();

if (app.Environment.IsDevelopment()) app.UseCors("AllowLocalhost");

app.UseApiLocalization(requireOpenApiLocale: true);

app.MapServiceHealthChecks();
app.MapReverseProxy();
app.MapApi();

app.UseSwaggerUI(options =>
{
    options.SwaggerEndpoint("/api/openapi.json", "API Gateway");
    var keycloakOptions = app.Services.GetRequiredService<IOptions<KeycloakOptions>>().Value;
    options.OAuthClientId(keycloakOptions.Audience);
    options.OAuthScopes("openid");
    options.OAuthUsePkce();
});

await app.RunAsync();
