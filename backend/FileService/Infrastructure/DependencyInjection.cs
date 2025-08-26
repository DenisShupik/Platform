using Amazon.Runtime;
using Amazon.S3;
using FileService.Infrastructure.Options;
using FluentValidation;
using Microsoft.Extensions.Options;
using OpenTelemetry.Trace;
using SharedKernel.Infrastructure.Extensions;

namespace FileService.Infrastructure;

public static class DependencyInjection
{
    public static void AddInfrastructureServices(this IHostApplicationBuilder builder)
    {
        builder.Services
            .AddValidatorsFromAssembly(typeof(DependencyInjection).Assembly, ServiceLifetime.Singleton)
            .RegisterOptions<S3Options, S3OptionsValidator>(builder.Configuration);

        builder.Services.AddSingleton<IAmazonS3>(sp =>
        {
            var options = sp.GetRequiredService<IOptions<S3Options>>().Value;
            var config = new AmazonS3Config
            {
                ServiceURL = options.ServiceURL,
                ForcePathStyle = true
            };
            var credentials = new BasicAWSCredentials(options.AccessKey, options.SecretKey);
            return new AmazonS3Client(credentials, config);
        });

        builder.Services
            .RegisterOpenTelemetry(builder.Environment.ApplicationName)
            .WithTracing(tracing => tracing.AddAWSInstrumentation());
    }
}