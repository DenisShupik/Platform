using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using System.ComponentModel.DataAnnotations;

namespace SharedKernel.Presentation.Extensions;

public static class HttpContextExtensions
{
    private const string ErrorMessage = "Invalid body";

    public static async ValueTask<T> GetRequiredBodyFromJsonAsync<T>(this HttpContext context) where T : notnull
    {
        var jsonOptions = context.RequestServices.GetService<IOptions<JsonOptions>>()?.Value;
        T? maybeBody;
        try
        {
            maybeBody = await context.Request.ReadFromJsonAsync<T>(
                jsonOptions?.JsonSerializerOptions,
                context.RequestAborted);
        }
        catch
        {
            throw new ValidationException(ErrorMessage);
        }

        return maybeBody ??
               throw new ValidationException(ErrorMessage);
    }
}