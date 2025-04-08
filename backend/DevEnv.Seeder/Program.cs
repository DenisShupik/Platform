using DevEnv.Seeder;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using SharedKernel.Options;
using SharedKernel.Presentation.Extensions.ServiceCollectionExtensions;

var builder = Host.CreateApplicationBuilder(args);

builder.Services
    .RegisterOptions<KeycloakOptions, KeycloakOptionsValidator>(builder.Configuration);

builder.Services.AddHttpClient<UserTokenService>();
builder.Services.AddHttpClient<ServiceTokenService>();
builder.Services.AddTransient<AuthenticationMessageHandler<UserTokenService>>();
builder.Services.AddTransient<AuthenticationMessageHandler<ServiceTokenService>>();

builder.Services.AddHttpClient<KeycloakClient>()
    .AddHttpMessageHandler<AuthenticationMessageHandler<ServiceTokenService>>();

builder.Services.AddHttpClient<ApiClient>()
    .AddHttpMessageHandler<AuthenticationMessageHandler<UserTokenService>>();

builder.Services.AddHostedService<Seeder>();

var host = builder.Build();

await host.RunAsync();