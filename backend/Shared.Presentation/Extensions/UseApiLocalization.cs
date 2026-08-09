using Microsoft.AspNetCore.Builder;
using Shared.Presentation.Middleware;

namespace Shared.Presentation.Extensions;

public static class UseApiLocalizationExtension
{
    public static IApplicationBuilder UseApiLocalization(this IApplicationBuilder app)
    {
        app.UseRequestLocalization();
        app.UseMiddleware<RequireApiLocaleMiddleware>();
        return app;
    }
}
