using System.Reflection;
using FluentValidation;
using LinqToDB.EntityFrameworkCore;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;
using SharedKernel.Filters;
using SharedKernel.Interfaces;
using SharedKernel.Options;
using SharpGrip.FluentValidation.AutoValidation.Endpoints.Extensions;
using Swashbuckle.AspNetCore.Filters;
using Swashbuckle.AspNetCore.SwaggerGen;

namespace SharedKernel.Extensions;

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

        LinqToDBForEFTools.Initialize();

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
                // var unauthorized = ReasonPhrases.GetReasonPhrase(StatusCodes.Status401Unauthorized);
                // options.Events = new JwtBearerEvents
                // {
                //     OnChallenge = context =>
                //     {
                //         context.Response.StatusCode = StatusCodes.Status401Unauthorized;
                //         context.Response.ContentType = MediaTypeNames.Application.Json;
                //         return context.Response.WriteAsync(unauthorized);
                //     },
                // };
            });

        return services;
    }

    public static IServiceCollection RegisterSwaggerGen(
        this IServiceCollection services
    )
    {
        services
            .AddFluentValidationAutoValidation()
            .AddEndpointsApiExplorer()
            .AddSwaggerGen();

        services.AddOptions<SwaggerGenOptions>()
            .Configure<IOptions<KeycloakOptions>>((options, keycloakOptions) =>
                {
                    options.AddSecurityDefinition("oauth2", new OpenApiSecurityScheme
                    {
                        Type = SecuritySchemeType.OpenIdConnect,
                        OpenIdConnectUrl = new Uri(keycloakOptions.Value.MetadataAddress)
                    });


                    options.SupportNonNullableReferenceTypes();
                    options.UseAllOfToExtendReferenceSchemas();
                    options.DescribeAllParametersInCamelCase();
                    options.OperationFilter<SecurityRequirementsOperationFilter>();
                    options.OperationFilter<AddInternalErrorResultOperationFilter>();
                    options.OperationFilter<AddOperationIdOperationFilter>();
                    options.ParameterFilter<SortParameterFilter>();

                    options.SchemaFilter<AddRequiredSchemaFilter>();

                    var xmlFiles = Directory.GetFiles(AppContext.BaseDirectory, "*.xml", SearchOption.TopDirectoryOnly)
                        .ToList();
                    foreach (var xmlFilePath in xmlFiles
                                 .Select(fileName => Path.Combine(AppContext.BaseDirectory, fileName))
                                 .Where(File.Exists))
                    {
                        options.IncludeXmlComments(xmlFilePath, includeControllerXmlComments: true);
                    }
                }
            );

        return services;
    }
}