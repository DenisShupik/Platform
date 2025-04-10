using DevEnv.Seeder.Services;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using SharedKernel.Options;
using SharedKernel.Presentation.Extensions.ServiceCollectionExtensions;

var builder = Host.CreateApplicationBuilder(args);

builder.Services
    .RegisterOptions<KeycloakOptions, KeycloakOptionsValidator>(builder.Configuration);

builder.Services.AddHttpClient<UserTokenService>();
builder.Services.AddHttpClient<ServiceTokenService>();
builder.Services.AddTransient<UserTokenService.Handler>();
builder.Services.AddTransient<ServiceTokenService.Handler>();

builder.Services.AddHttpClient<KeycloakClient>()
    .AddHttpMessageHandler<ServiceTokenService.Handler>();

builder.Services.AddHttpClient<ApiClient>()
    .AddHttpMessageHandler<UserTokenService.Handler>();

builder.Services.AddHostedService<Seeder>();

var host = builder.Build();

await host.RunAsync();