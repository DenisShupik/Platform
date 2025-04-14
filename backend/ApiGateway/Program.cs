using ApiGateway.Extensions;
using Microsoft.Extensions.Options;
using Yarp.ReverseProxy.Swagger;

var builder = WebApplication.CreateBuilder(args);

Console.WriteLine($"Environment: {builder.Environment.EnvironmentName}");

var configuration = builder.Configuration.GetRequiredSection("ReverseProxy");

builder.Services
    .AddReverseProxy()
    .LoadFromConfig(configuration)
    .RegisterSwagger(configuration)
    ;

builder.WebHost.UseKestrelHttpsConfiguration();

var app = builder.Build();

app.UseSwagger();
app.UseSwaggerUI(options =>
{
    var config = app.Services.GetRequiredService<IOptionsMonitor<ReverseProxyDocumentFilterConfig>>().CurrentValue;
    options.ConfigureSwaggerEndpoints(config);
});

app.MapReverseProxy();

await app.RunAsync();