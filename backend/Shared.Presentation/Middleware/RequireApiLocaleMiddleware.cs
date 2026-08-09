using Microsoft.AspNetCore.Localization;
using Microsoft.AspNetCore.Http;
using Microsoft.Net.Http.Headers;
using Shared.Domain.ValueObjects;
using Shared.Presentation.Errors;
using Shared.Presentation.Localization;

namespace Shared.Presentation.Middleware;

public sealed class RequireApiLocaleMiddleware
{
    private static readonly PathString OpenApiPath = new("/api/openapi.json");
    private readonly RequestDelegate _next;

    public RequireApiLocaleMiddleware(RequestDelegate next)
    {
        _next = next;
    }

    public async Task InvokeAsync(HttpContext context)
    {
        if (!RequiresExplicitLocale(context.Request.Path))
        {
            await _next(context);
            return;
        }

        var requestCulture = context.Features.Get<IRequestCultureFeature>();
        if (requestCulture?.Provider is not StrictAcceptLanguageRequestCultureProvider)
        {
            context.Response.Headers.Remove(HeaderNames.ContentLanguage);
            ApiResponseLocalization.AddAcceptLanguageVary(context.Response);
            context.Response.StatusCode = StatusCodes.Status406NotAcceptable;

            var hasAcceptLanguage = context.Request.Headers.ContainsKey(HeaderNames.AcceptLanguage);
            object error = hasAcceptLanguage
                ? new UnsupportedLocaleError(Locale.SupportedCodes)
                : new LocaleRequiredError(Locale.SupportedCodes);

            await context.Response.WriteAsJsonAsync(error, context.RequestAborted);
            return;
        }

        var locale = requestCulture.RequestCulture.UICulture.Name;
        ApiResponseLocalization.Apply(context.Response, locale);
        context.Response.OnStarting(() =>
        {
            ApiResponseLocalization.Apply(context.Response, locale);
            return Task.CompletedTask;
        });

        await _next(context);
    }

    private static bool RequiresExplicitLocale(PathString path) =>
        path.StartsWithSegments("/api") && path != OpenApiPath;
}
