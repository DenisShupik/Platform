using DevEnv.Seeder.Services;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using SharedKernel.Options;
using SharedKernel.Presentation.Extensions.ServiceCollectionExtensions;
using SharedKernel.Tests.Services;

var builder = Host.CreateApplicationBuilder(args);

builder.Services
    .RegisterOptions<KeycloakOptions, KeycloakOptionsValidator>(builder.Configuration);

var apiGatewayUri = builder.Configuration.GetValue<Uri>("services:api-gateway:http:0");

builder.Services.AddSingleton<Fixture>();
builder.Services.AddSingleton<UserTokenService>();
builder.Services.AddHttpClient<ServiceTokenService>();
builder.Services.AddTransient(sp =>
{
    var fixture = sp.GetRequiredService<Fixture>();
    var userTokenService = sp.GetRequiredService<UserTokenService>();
    return new UserTokenService.Handler(userTokenService, fixture.GetRandomUser);
});

builder.Services.AddTransient<ServiceTokenService.Handler>();

builder.Services.AddHttpClient<KeycloakAdminClient>()
    .AddHttpMessageHandler<ServiceTokenService.Handler>();

builder.Services.AddHttpClient<CoreServiceClient>(httpClient => { httpClient.BaseAddress = apiGatewayUri; })
    .AddHttpMessageHandler<UserTokenService.Handler>();

builder.Services.AddHttpClient<FileServiceClient>(httpClient => { httpClient.BaseAddress = apiGatewayUri; })
    .AddHttpMessageHandler<UserTokenService.Handler>();

builder.Services.AddHostedService<Seeder>();

var host = builder.Build();

await host.RunAsync();