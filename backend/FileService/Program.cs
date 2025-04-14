using System.Reflection;
using Amazon.Runtime;
using Amazon.S3;
using FileService.Presentation.Extensions;
using FileService.Presentation.Options;
using FluentValidation;
using Microsoft.Extensions.Options;
using SharedKernel.Extensions;
using SharedKernel.Presentation.Extensions.ServiceCollectionExtensions;

var builder = WebApplication.CreateBuilder(args);

builder.Services
    .AddValidatorsFromAssembly(Assembly.Load(nameof(SharedKernel)), ServiceLifetime.Singleton)
    .AddValidatorsFromAssemblyContaining<Program>(ServiceLifetime.Singleton)
    .RegisterAuthenticationSchemes(builder.Configuration)
    ;

builder.Services.RegisterOptions<S3Options, S3OptionsValidator>(builder.Configuration);
builder.Services.AddDefaultAWSOptions(sp =>
{
    var options = sp.GetRequiredService<IOptions<S3Options>>().Value;
    var awsOptions = builder.Configuration.GetAWSOptions(nameof(S3Options));
    awsOptions.Credentials = new BasicAWSCredentials(options.AccessKey, options.SecretKey);
    awsOptions.DefaultClientConfig.ServiceURL = options.ServiceURL;
    return awsOptions;
});
builder.Services.AddAWSService<IAmazonS3>();

builder.Services.RegisterSwaggerGen();

builder.WebHost.UseKestrelHttpsConfiguration();

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