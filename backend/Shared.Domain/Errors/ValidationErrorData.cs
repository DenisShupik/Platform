namespace Shared.Domain.Errors;

public sealed record ValidationErrorData(string Code, IReadOnlyList<string> Parameters)
{
    public static ValidationErrorData Create(string code, params object[] parameters) =>
        new(code, parameters.Select(static parameter =>
            Convert.ToString(parameter, System.Globalization.CultureInfo.InvariantCulture) ?? string.Empty).ToArray());
}
