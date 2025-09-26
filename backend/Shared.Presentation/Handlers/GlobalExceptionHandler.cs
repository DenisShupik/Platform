using System.Text.Json;
using Microsoft.AspNetCore.Diagnostics;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Shared.Presentation.Exceptions;
using Vogen;

namespace Shared.Presentation.Handlers;

public sealed class GlobalExceptionHandler : IExceptionHandler
{
    public async ValueTask<bool> TryHandleAsync(HttpContext httpContext, Exception exception,
        CancellationToken cancellationToken)
    {
        var problemDetails = new ProblemDetails
        {
            Instance = httpContext.Request.Path
        };

        switch (exception)
        {
            case FluentValidation.ValidationException typedException:
            {
                problemDetails.Title = "One or more validation errors occurred";
                problemDetails.Type = "https://tools.ietf.org/html/rfc7231#section-6.5.1";
                httpContext.Response.StatusCode = StatusCodes.Status400BadRequest;
                var validationErrors = typedException.Errors.Select(error => error.ErrorMessage).ToList();
                problemDetails.Extensions.Add("errors", validationErrors);
                break;
            }
            case System.ComponentModel.DataAnnotations.ValidationException typedException:
                problemDetails.Title = "One or more validation errors occurred";
                problemDetails.Type = "https://tools.ietf.org/html/rfc7231#section-6.5.1";
                httpContext.Response.StatusCode = StatusCodes.Status400BadRequest;
                problemDetails.Extensions.Add("errors", new[] { typedException.Message });
                break;
            case ValidationException typedException:
                problemDetails.Title = "One or more validation errors occurred";
                problemDetails.Type = "https://tools.ietf.org/html/rfc7231#section-6.5.1";
                httpContext.Response.StatusCode = StatusCodes.Status400BadRequest;
                problemDetails.Extensions.Add("errors", typedException.Errors);
                break;
            case BadHttpRequestException typedException:
            {
                if (typedException.InnerException is JsonException
                    {
                        InnerException: ValueObjectValidationException valueObjectValidationException
                    } jsonException)
                {
                    problemDetails.Title = "One or more validation errors occurred";
                    problemDetails.Type = "https://tools.ietf.org/html/rfc7231#section-6.5.1";
                    httpContext.Response.StatusCode = StatusCodes.Status400BadRequest;
                    problemDetails.Extensions.Add("errors",
                        new[] { $"{jsonException.Path}: {valueObjectValidationException.Message}" });
                }

                break;
            }
            default:
                problemDetails.Title = exception.Message;
                break;
        }

        problemDetails.Status = httpContext.Response.StatusCode;
        await httpContext.Response.WriteAsJsonAsync(problemDetails, cancellationToken);
        return true;
    }
}