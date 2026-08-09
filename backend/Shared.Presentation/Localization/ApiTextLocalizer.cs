using Microsoft.Extensions.Localization;
using Shared.Domain.Errors;

namespace Shared.Presentation.Localization;

public sealed class ApiTextLocalizer
{
    private readonly IStringLocalizer<ApiResources> _localizer;

    public ApiTextLocalizer(IStringLocalizer<ApiResources> localizer)
    {
        _localizer = localizer;
    }

    public string Get(string key, params object[] parameters)
    {
        var value = parameters.Length == 0
            ? _localizer[key]
            : _localizer[key, parameters];

        if (value.ResourceNotFound)
            throw new InvalidOperationException($"Missing localized API resource '{key}' for culture " +
                                                $"'{System.Globalization.CultureInfo.CurrentUICulture.Name}'.");

        return value.Value;
    }

    public string Get(ValidationErrorData error) =>
        Get(error.Code, error.Parameters.Cast<object>().ToArray());
}
