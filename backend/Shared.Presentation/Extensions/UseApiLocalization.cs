using Microsoft.AspNetCore.Builder;
using Shared.Presentation.Middleware;

namespace Shared.Presentation.Extensions;

public static class UseApiLocalizationExtension
{
    public static IApplicationBuilder UseApiLocalization(
        this IApplicationBuilder app,
        bool requireOpenApiLocale = false)
    {
        app.UseRequestLocalization();
        app.UseMiddleware<RequireApiLocaleMiddleware>(requireOpenApiLocale);
        return app;
    }
}
