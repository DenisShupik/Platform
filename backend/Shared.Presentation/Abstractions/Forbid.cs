using System.Reflection;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.Metadata;
using Microsoft.Extensions.DependencyInjection;
using Shared.Presentation.Helpers;
using HttpResultsHelper = Shared.Presentation.Helpers.HttpResultsHelper;
using ILoggerFactory = Microsoft.Extensions.Logging.ILoggerFactory;

namespace Shared.Presentation.Abstractions;

/// <summary>
/// An <see cref="IResult"/> that on execution will write an object to the response
/// with Forbidden (403) status code.
/// </summary>
public sealed class Forbid<T> : IResult, IEndpointMetadataProvider, IStatusCodeHttpResult
{
    /// <summary>
    /// Initializes a new instance of the <see cref="Forbid{T}"/> class with the values.
    /// </summary>
    public Forbid(T? value)
    {
        Value = value;
    }

    /// <summary>
    /// Gets the value to be written to the response.
    /// </summary>
    public T? Value { get; }

    /// <summary>
    /// Gets the HTTP status code: <see cref="StatusCodes.Status403Forbidden"/>
    /// </summary>
    public int StatusCode => StatusCodes.Status403Forbidden;

    int? IStatusCodeHttpResult.StatusCode => StatusCode;

    /// <inheritdoc/>
    public async Task ExecuteAsync(HttpContext httpContext)
    {
        ArgumentNullException.ThrowIfNull(httpContext);

        var loggerFactory = httpContext.RequestServices.GetRequiredService<ILoggerFactory>();
        var logger = loggerFactory.CreateLogger("Microsoft.AspNetCore.Http.Result.ForbidObjectResult");

        HttpResultsHelper.Log.WritingResultAsStatusCode(logger, StatusCode);
        httpContext.Response.StatusCode = StatusCode;

        if (Value is not null)
        {
            httpContext.Response.ContentType = "application/json";
            await httpContext.Response.WriteAsJsonAsync(Value);
        }
    }

    /// <inheritdoc/>
    static void IEndpointMetadataProvider.PopulateMetadata(MethodInfo method, EndpointBuilder builder)
    {
        ArgumentNullException.ThrowIfNull(method);
        ArgumentNullException.ThrowIfNull(builder);

        builder.Metadata.Add(new ProducesResponseTypeMetadata(StatusCodes.Status403Forbidden, typeof(T)));
    }
}