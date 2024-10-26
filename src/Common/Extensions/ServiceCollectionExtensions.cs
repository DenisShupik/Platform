using System.Net.Mime;
using Common.Interfaces;
using Common.Options;
using FluentValidation;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.WebUtilities;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Protocols.OpenIdConnect;
using Microsoft.IdentityModel.Tokens;

namespace Common.Extensions;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection RegisterOptions<TOptions>(
        this IServiceCollection services,
        IConfiguration configuration
    )
        where TOptions : class
    {
        services
            .AddOptions<TOptions>()
            .Bind(configuration.GetSection(typeof(TOptions).Name))
            .Validate<IValidator<TOptions>>((options, validator) =>
            {
                var result = validator.Validate(options);
                if (!result.IsValid) throw new ValidationException(result.ToString());
                return true;
            })
            .ValidateOnStart();
        return services;
    }

    public static IServiceCollection RegisterPooledDbContextFactory<TDbContext, TDbOptions>(
        this IServiceCollection services,
        string schema
    )
        where TDbContext : DbContext
        where TDbOptions : class, IDbOptions
    {
        services.AddPooledDbContextFactory<TDbContext>(
            (provider, options) =>
            {
                var dbOptions = provider.GetRequiredService<IOptions<TDbOptions>>().Value;
                var loggerFactory = provider.GetRequiredService<ILoggerFactory>();
                options
                    .UseNpgsql(dbOptions.ConnectionString,
                        builder => { builder.MigrationsHistoryTable("migrations_history", schema); })
                    .UseLoggerFactory(loggerFactory)
                    .UseSnakeCaseNamingConvention();
            });

        return services;
    }

    public static IServiceCollection RegisterAuthenticationSchemes(
        this IServiceCollection services,
        IConfiguration configuration
    )
    {
        services
            .RegisterOptions<KeycloakOptions>(configuration)
            .AddAuthentication()
            .AddJwtBearer();

        services.AddAuthorization();

        services
            .AddOptions<JwtBearerOptions>()
            .Configure<IOptions<KeycloakOptions>>((options, keycloakOptions) =>
            {
                var host = keycloakOptions.Value.Host;
                var issuers = keycloakOptions.Value.Issuers;
                options.Configuration = new OpenIdConnectConfiguration
                {
                    Issuer = host
                };
                options.TokenValidationParameters = new TokenValidationParameters
                {
                    ValidateAudience = true,
                    ValidateIssuer = true,
                    ValidateLifetime = true,
                    ValidateIssuerSigningKey = true,
                    ValidIssuers = issuers,
                    ClockSkew = TimeSpan.Zero
                };
                var unauthorized = ReasonPhrases.GetReasonPhrase(StatusCodes.Status401Unauthorized);
                options.Events = new JwtBearerEvents
                {
                    OnChallenge = context =>
                    {
                        context.Response.StatusCode = StatusCodes.Status401Unauthorized;
                        context.Response.ContentType = MediaTypeNames.Application.Json;
                        return context.Response.WriteAsync(unauthorized);
                    }
                    //OnMessageReceived = context =>
                    //{
                    //    if (context.HttpContext.WebSockets.IsWebSocketRequest)
                    //    {

                    //    }

                    //    return Task.CompletedTask;
                    //}
                };
            });

        return services;
    }
}