using Microsoft.Extensions.Options;
using Swashbuckle.AspNetCore.SwaggerGen;
using Yarp.ReverseProxy.Swagger;
using Yarp.ReverseProxy.Swagger.Extensions;

namespace ApiGateway.Presentation.Extensions;

public static class ReverseProxyBuilderExtensions
{
    public static IReverseProxyBuilder RegisterSwagger(this IReverseProxyBuilder builder,
        IConfigurationSection configuration)
    {
        builder.Services
            .AddOptions<SwaggerGenOptions>()
            .Configure<IOptionsMonitor<ReverseProxyDocumentFilterConfig>>(
                (options, reverseProxyDocumentFilterConfigOptions) =>
                {
                    var filterDescriptors = new List<FilterDescriptor>();

                    options.ConfigureSwaggerDocs(reverseProxyDocumentFilterConfigOptions.CurrentValue);

                    filterDescriptors.Add(new FilterDescriptor
                    {
                        Type = typeof(ReverseProxyDocumentFilter),
                        Arguments = []
                    });

                    options.DocumentFilterDescriptors = filterDescriptors;
                    var xmlFiles = Directory.GetFiles(AppContext.BaseDirectory, "*.xml", SearchOption.TopDirectoryOnly)
                        .ToList();
                    foreach (var xmlFilePath in xmlFiles
                                 .Select(fileName => Path.Combine(AppContext.BaseDirectory, fileName))
                                 .Where(File.Exists))
                    {
                        options.IncludeXmlComments(xmlFilePath, includeControllerXmlComments: true);
                    }
                });

        builder.Services
            .AddEndpointsApiExplorer()
            .AddSwaggerGen();

        builder.AddSwagger(configuration);

        return builder;
    }
}