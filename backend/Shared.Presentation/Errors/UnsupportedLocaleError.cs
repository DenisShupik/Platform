using Shared.Domain.Abstractions.Errors;

namespace Shared.Presentation.Errors;

public sealed record UnsupportedLocaleError(IReadOnlyList<string> SupportedLocales) : Error;
