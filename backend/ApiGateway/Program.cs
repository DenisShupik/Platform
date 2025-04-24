using ApiGateway.Extensions;
using Microsoft.Extensions.Options;
using SharedKernel.Presentation.Options;
using Yarp.ReverseProxy.Swagger;

var builder = WebApplication.CreateBuilder(args);

Console.WriteLine($"Environment: {builder.Environment.EnvironmentName}");

var configuration = builder.Configuration.GetRequiredSection("ReverseProxy");

builder.Services
    .AddReverseProxy()
    .LoadFromConfig(configuration)
    .RegisterSwagger(configuration)
    ;

if (builder.Environment.IsDevelopment())
{
    builder.Services.AddCors(options =>
    {
        options.AddPolicy("AllowLocalhost",
            b => b.WithOrigins("http://localhost:4173", "http://localhost:5173")
                .AllowAnyMethod()
                .AllowAnyHeader());
    });
}

var app = builder.Build();

if (app.Environment.IsDevelopment()) app.UseCors("AllowLocalhost");

app.UseSwagger();
app.UseSwaggerUI(options =>
{
    var config = app.Services.GetRequiredService<IOptionsMonitor<ReverseProxyDocumentFilterConfig>>().CurrentValue;
    options.ConfigureSwaggerEndpoints(config);
    var keycloakOptions = app.Services.GetRequiredService<IOptions<KeycloakOptions>>().Value;
    options.OAuthClientId(keycloakOptions.Audience);
    options.OAuthUsePkce();
});

app.MapReverseProxy();

await app.RunAsync();