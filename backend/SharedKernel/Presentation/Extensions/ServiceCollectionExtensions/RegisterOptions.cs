using FluentValidation;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using ValidationException = System.ComponentModel.DataAnnotations.ValidationException;

namespace SharedKernel.Presentation.Extensions.ServiceCollectionExtensions;

public static partial class ServiceCollectionExtensions
{
    public static IServiceCollection RegisterOptions<TOptions, TValidator>(
        this IServiceCollection services,
        IConfiguration configuration)
        where TOptions : class
        where TValidator : class, IValidator<TOptions>
    {
        services.AddSingleton<IValidator<TOptions>, TValidator>();
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
}