using Microsoft.Extensions.Options;
using Microsoft.OpenApi.Models;
using Swashbuckle.AspNetCore.SwaggerGen;
using Swashbuckle.AspNetCore.SwaggerUI;
using Yarp.ReverseProxy.Swagger;
using Yarp.ReverseProxy.Swagger.Extensions;

var builder = WebApplication.CreateBuilder(args);

var configuration = builder.Configuration.GetSection("ReverseProxy");

builder.Services.AddTransient<IConfigureOptions<SwaggerGenOptions>, ConfigureSwaggerOptions>();

builder.Services
    .AddEndpointsApiExplorer()
    .AddSwaggerGen();

builder.Services
    .AddReverseProxy()
    .LoadFromConfig(configuration)
    .AddSwagger(configuration)
    ;

builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowLocalhost",
        b => b.WithOrigins("https://localhost:4173", "https://localhost:5173")
            .AllowAnyMethod()
            .AllowAnyHeader());
});

builder.WebHost.UseKestrelHttpsConfiguration();

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI(options =>
    {
        var config = app.Services.GetRequiredService<IOptionsMonitor<ReverseProxyDocumentFilterConfig>>().CurrentValue;
        options.ConfigureSwaggerEndpoints(config);
    });
}

app.UseCors("AllowLocalhost");

app.MapReverseProxy();

await app.RunAsync();


public class ConfigureSwaggerOptions(
    IOptionsMonitor<ReverseProxyDocumentFilterConfig> reverseProxyDocumentFilterConfigOptions)
    : IConfigureOptions<SwaggerGenOptions>
{
    private readonly ReverseProxyDocumentFilterConfig _reverseProxyDocumentFilterConfig = reverseProxyDocumentFilterConfigOptions.CurrentValue;

    public void Configure(SwaggerGenOptions options)
    {
        var filterDescriptors = new List<FilterDescriptor>();

        options.ConfigureSwaggerDocs(_reverseProxyDocumentFilterConfig);

        filterDescriptors.Add(new FilterDescriptor
        {
            Type = typeof(ReverseProxyDocumentFilter),
            Arguments = []
        });

        options.DocumentFilterDescriptors = filterDescriptors;
    }
}

public static class SwaggerExtensions
{
    public static void ConfigureSwaggerEndpoints(
        this SwaggerUIOptions options,
        ReverseProxyDocumentFilterConfig config
    )
    {
        if (config.Swagger.IsCommonDocument)
        {
            var name = config.Swagger.CommonDocumentName;
            options.SwaggerEndpoint($"/swagger/{name}/swagger.json", name);
        }
        else
        {
            foreach (var cluster in config.Clusters)
            {
                var name = cluster.Key;
                options.SwaggerEndpoint($"/swagger/{name}/swagger.json", name);
            }
        }
    }
    
    public static void ConfigureSwaggerDocs(
        this SwaggerGenOptions options,
        ReverseProxyDocumentFilterConfig config
    )
    {
        if (config.Swagger.IsCommonDocument)
        {
            var name = config.Swagger.CommonDocumentName;
            options.SwaggerDoc(name, new OpenApiInfo {Title = name, Version = name});
        }
        else
        {
            foreach (var cluster in config.Clusters)
            {
                var name = cluster.Key;
                options.SwaggerDoc(name, new OpenApiInfo {Title = name, Version = name});
            }
        }
    }
}