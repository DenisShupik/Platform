using System.Reflection;
using Amazon.Runtime;
using Amazon.S3;
using FileService.Infrastructure;
using FileService.Presentation.Extensions;
using FileService.Presentation.Options;
using FluentValidation;
using Microsoft.Extensions.Options;
using SharedKernel.Presentation.Extensions;
using SharedKernel.Presentation.Extensions.ServiceCollectionExtensions;

var builder = WebApplication.CreateBuilder(args);

builder.Services
    .AddValidatorsFromAssembly(Assembly.Load(nameof(SharedKernel)), ServiceLifetime.Singleton)
    .AddValidatorsFromAssemblyContaining<Program>(ServiceLifetime.Singleton)
    .RegisterAuthenticationSchemes(builder.Configuration)
    ;

builder.Services.RegisterOptions<S3Options, S3OptionsValidator>(builder.Configuration);

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

builder.Services.RegisterSwaggerGen();

builder.AddInfrastructureServices();

var app = builder.Build();

app
    .UseSwagger()
    .UseSwaggerUI();

app
    .UseAuthentication()
    .UseAuthorization();

app.MapApi();

app.Logger.StartingApp();

await app.RunAsync();