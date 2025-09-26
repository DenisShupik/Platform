using FluentValidation;
using Microsoft.Extensions.Configuration;

namespace DevEnv.Extensions;

internal static class DistributedApplicationBuilderExtensions
{
    public static T GetOptions<T, TValidator>(this IDistributedApplicationBuilder builder)
        where TValidator : AbstractValidator<T>, new()
    {
        var options = builder.Configuration.GetRequiredSection(typeof(T).Name).Get<T>();
        ArgumentNullException.ThrowIfNull(options);

        var validator = new TValidator();
        var result = validator.Validate(options);
        return !result.IsValid ? throw new ValidationException(result.ToString()) : options;
    }
}