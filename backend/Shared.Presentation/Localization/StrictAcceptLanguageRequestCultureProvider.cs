using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Localization;
using Shared.Domain.ValueObjects;

namespace Shared.Presentation.Localization;

public sealed class StrictAcceptLanguageRequestCultureProvider : RequestCultureProvider
{
    public override Task<ProviderCultureResult?> DetermineProviderCultureResult(HttpContext httpContext)
    {
        ArgumentNullException.ThrowIfNull(httpContext);

        IList<Microsoft.Net.Http.Headers.StringWithQualityHeaderValue>? acceptedLanguages;
        try
        {
            acceptedLanguages = httpContext.Request.GetTypedHeaders().AcceptLanguage;
        }
        catch (FormatException)
        {
            return Task.FromResult<ProviderCultureResult?>(null);
        }

        var selectedLocale = acceptedLanguages?
            .Select((language, index) => new
            {
                Code = Locale.SupportedCodes.FirstOrDefault(code =>
                    code.Equals(language.Value.ToString(), StringComparison.OrdinalIgnoreCase)),
                Index = index,
                Quality = language.Quality ?? 1d
            })
            .Where(candidate => candidate.Code is not null && candidate.Quality > 0d)
            .OrderByDescending(candidate => candidate.Quality)
            .ThenBy(candidate => candidate.Index)
            .Select(candidate => candidate.Code)
            .FirstOrDefault();

        return selectedLocale is null
            ? Task.FromResult<ProviderCultureResult?>(null)
            : Task.FromResult<ProviderCultureResult?>(new ProviderCultureResult(selectedLocale));
    }
}
