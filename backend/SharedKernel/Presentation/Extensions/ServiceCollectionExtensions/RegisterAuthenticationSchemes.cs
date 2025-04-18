﻿using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using SharedKernel.Presentation.Options;

namespace SharedKernel.Presentation.Extensions.ServiceCollectionExtensions;

public static partial class ServiceCollectionExtensions
{
    public static IServiceCollection RegisterAuthenticationSchemes(
        this IServiceCollection services,
        IConfiguration configuration
    )
    {
        services
            .RegisterOptions<KeycloakOptions, KeycloakOptionsValidator>(configuration)
            .AddAuthentication()
            .AddJwtBearer();

        services.AddAuthorization();

        services
            .AddOptions<JwtBearerOptions>(JwtBearerDefaults.AuthenticationScheme)
            .Configure<IOptions<KeycloakOptions>>((options, keycloakOptions) =>
            {
                options.MetadataAddress = keycloakOptions.Value.MetadataAddress;
                options.RequireHttpsMetadata = false;
                options.TokenValidationParameters = new TokenValidationParameters
                {
                    ValidateAudience = true,
                    ValidateIssuer = true,
                    ValidateLifetime = true,
                    ValidateIssuerSigningKey = true,
                    ValidIssuer = keycloakOptions.Value.Issuer,
                    ValidAudience = keycloakOptions.Value.Audience,
                    ClockSkew = TimeSpan.Zero
                };
            });

        return services;
    }
}