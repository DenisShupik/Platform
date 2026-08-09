using Shared.Domain.Abstractions.Errors;

namespace Shared.Presentation.Errors;

public sealed record LocaleRequiredError(IReadOnlyList<string> SupportedLocales) : Error;
