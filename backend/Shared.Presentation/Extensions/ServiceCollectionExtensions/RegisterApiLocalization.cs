using System.Globalization;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Localization;
using Microsoft.Extensions.DependencyInjection;
using Shared.Domain.ValueObjects;
using Shared.Presentation.Localization;

namespace Shared.Presentation.Extensions;

public static class RegisterApiLocalizationExtension
{
    public static IServiceCollection RegisterApiLocalization(this IServiceCollection services)
    {
        services.AddLocalization(options => options.ResourcesPath = "Resources");
        services.AddSingleton<ApiTextLocalizer>();

        services.Configure<RequestLocalizationOptions>(options =>
        {
            var supportedCultures = Locale.SupportedCodes
                .Select(CultureInfo.GetCultureInfo)
                .ToArray();

            options.DefaultRequestCulture = new RequestCulture(Locale.EnglishCode);
            options.SupportedCultures = supportedCultures;
            options.SupportedUICultures = supportedCultures;
            options.FallBackToParentCultures = false;
            options.FallBackToParentUICultures = false;
            options.ApplyCurrentCultureToResponseHeaders = true;
            options.RequestCultureProviders = [new StrictAcceptLanguageRequestCultureProvider()];
        });

        return services;
    }
}
