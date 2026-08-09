using Shared.Domain.Errors;

namespace Shared.Domain.Helpers;

public static class ValidationErrorCodec
{
    private const string Prefix = "validation:";
    private const char Separator = '|';

    public static string Encode(string code, params object[] parameters)
    {
        var encodedParameters = parameters.Select(static parameter =>
            Uri.EscapeDataString(
                Convert.ToString(parameter, System.Globalization.CultureInfo.InvariantCulture) ?? string.Empty));

        return string.Join(Separator, [Prefix + code, .. encodedParameters]);
    }

    public static ValidationErrorData Decode(string encoded)
    {
        if (!encoded.StartsWith(Prefix, StringComparison.Ordinal))
            return ValidationErrorData.Create(ValidationErrorCodes.CannotParseInputValue);

        var parts = encoded.Split(Separator);
        var code = parts[0][Prefix.Length..];
        var parameters = parts
            .Skip(1)
            .Select(Uri.UnescapeDataString)
            .ToArray();

        return new ValidationErrorData(code, parameters);
    }
}
