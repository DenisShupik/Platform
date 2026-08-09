using System.Text.Json;
using System.Diagnostics;
using Microsoft.AspNetCore.Diagnostics;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Microsoft.IdentityModel.Tokens;
using Shared.Domain.Errors;
using Shared.Domain.Helpers;
using Shared.Presentation.Errors;
using Shared.Presentation.Exceptions;
using Shared.Presentation.Extensions;
using Shared.Presentation.Localization;
using Vogen;

namespace Shared.Presentation.Handlers;

public sealed class GlobalExceptionHandler : IExceptionHandler
{
    private const string ProblemJsonContentType = "application/problem+json";
    private const string BadRequestType = "https://www.rfc-editor.org/rfc/rfc9110#name-400-bad-request";
    private const string InternalServerErrorType =
        "https://www.rfc-editor.org/rfc/rfc9110#name-500-internal-server-error";
    private const string PayloadTooLargeType =
        "https://www.rfc-editor.org/rfc/rfc9110#name-413-content-too-large";

    private readonly ApiTextLocalizer _localizer;
    private readonly ILogger<GlobalExceptionHandler> _logger;

    public GlobalExceptionHandler(
        ApiTextLocalizer localizer,
        ILogger<GlobalExceptionHandler> logger)
    {
        _localizer = localizer;
        _logger = logger;
    }

    public async ValueTask<bool> TryHandleAsync(
        HttpContext httpContext,
        Exception exception,
        CancellationToken cancellationToken)
    {
        ApiResponseLocalization.Apply(
            httpContext.Response,
            System.Globalization.CultureInfo.CurrentUICulture.Name);

        switch (exception)
        {
            case UnauthorizedException unauthorizedException:
                httpContext.Response.StatusCode = StatusCodes.Status401Unauthorized;
                await httpContext.Response.WriteAsJsonAsync(unauthorizedException.Error, cancellationToken);
                return true;

            case SecurityTokenExpiredException:
                httpContext.Response.StatusCode = StatusCodes.Status401Unauthorized;
                await httpContext.Response.WriteAsJsonAsync(new TokenExpiredError(), cancellationToken);
                return true;

            case FluentValidation.ValidationException validationException:
            {
                var errors = validationException.Errors
                    .GroupBy(error => error.PropertyName, StringComparer.Ordinal)
                    .ToDictionary(
                        group => group.Key,
                        _ => ValidationErrorData.Create(ValidationErrorCodes.ValidationRuleFailed),
                        StringComparer.Ordinal);

                await WriteValidationProblemAsync(httpContext, errors, cancellationToken);
                return true;
            }

            case System.ComponentModel.DataAnnotations.ValidationException validationException:
            {
                var errors = new Dictionary<string, ValidationErrorData>(StringComparer.Ordinal)
                {
                    [HttpContextExtensions.BodyPropertyName] =
                        ValidationErrorCodec.Decode(validationException.Message)
                };
                await WriteValidationProblemAsync(httpContext, errors, cancellationToken);
                return true;
            }

            case ValidationException validationException:
            {
                var errors = validationException.Errors.ToDictionary(
                    pair => pair.Key,
                    pair => ValidationErrorCodec.Decode(pair.Value),
                    StringComparer.Ordinal);
                await WriteValidationProblemAsync(httpContext, errors, cancellationToken);
                return true;
            }

            case BadHttpRequestException
            {
                InnerException: JsonException
                {
                    InnerException: ValueObjectValidationException valueObjectValidationException
                } jsonException
            }:
            {
                var errors = new Dictionary<string, ValidationErrorData>(StringComparer.Ordinal)
                {
                    [jsonException.Path ?? HttpContextExtensions.BodyPropertyName] =
                        ValidationErrorCodec.Decode(valueObjectValidationException.Message)
                };
                await WriteValidationProblemAsync(httpContext, errors, cancellationToken);
                return true;
            }

            case JsonException:
            {
                var errors = new Dictionary<string, ValidationErrorData>(StringComparer.Ordinal)
                {
                    [HttpContextExtensions.BodyPropertyName] =
                        ValidationErrorData.Create(ValidationErrorCodes.InvalidJsonBody)
                };
                await WriteValidationProblemAsync(httpContext, errors, cancellationToken);
                return true;
            }

            case BadHttpRequestException badHttpRequestException:
                await WriteInvalidRequestProblemAsync(
                    httpContext,
                    badHttpRequestException.StatusCode,
                    cancellationToken);
                return true;

            default:
                _logger.LogError(exception, "Unhandled exception while processing {Method} {Path}",
                    httpContext.Request.Method, httpContext.Request.Path);
                await WriteUnexpectedProblemAsync(httpContext, cancellationToken);
                return true;
        }
    }

    private async Task WriteValidationProblemAsync(
        HttpContext httpContext,
        IReadOnlyDictionary<string, ValidationErrorData> errors,
        CancellationToken cancellationToken)
    {
        httpContext.Response.StatusCode = StatusCodes.Status400BadRequest;

        var localizedErrors = errors.ToDictionary(
            pair => pair.Key,
            pair => new ApiValidationError(
                pair.Value.Code,
                _localizer.Get(pair.Value),
                pair.Value.Parameters),
            StringComparer.Ordinal);

        var problemDetails = new ApiValidationProblemDetails
        {
            Code = ApiProblemCodes.ValidationFailed,
            Errors = localizedErrors,
            Instance = httpContext.Request.Path,
            Status = StatusCodes.Status400BadRequest,
            Title = _localizer.Get("validation_problem_title"),
            TraceId = GetTraceId(httpContext),
            Type = BadRequestType
        };

        await httpContext.Response.WriteAsJsonAsync(
            problemDetails,
            options: null,
            contentType: ProblemJsonContentType,
            cancellationToken);
    }

    private async Task WriteUnexpectedProblemAsync(
        HttpContext httpContext,
        CancellationToken cancellationToken)
    {
        httpContext.Response.StatusCode = StatusCodes.Status500InternalServerError;
        var problemDetails = new ApiProblemDetails
        {
            Code = ApiProblemCodes.UnexpectedError,
            Instance = httpContext.Request.Path,
            Status = StatusCodes.Status500InternalServerError,
            Title = _localizer.Get("unexpected_problem_title"),
            TraceId = GetTraceId(httpContext),
            Type = InternalServerErrorType
        };

        await httpContext.Response.WriteAsJsonAsync(
            problemDetails,
            options: null,
            contentType: ProblemJsonContentType,
            cancellationToken);
    }

    private async Task WriteInvalidRequestProblemAsync(
        HttpContext httpContext,
        int statusCode,
        CancellationToken cancellationToken)
    {
        var isPayloadTooLarge = statusCode == StatusCodes.Status413PayloadTooLarge;
        httpContext.Response.StatusCode = statusCode;

        var problemDetails = new ApiProblemDetails
        {
            Code = isPayloadTooLarge
                ? ApiProblemCodes.PayloadTooLarge
                : ApiProblemCodes.InvalidRequest,
            Instance = httpContext.Request.Path,
            Status = statusCode,
            Title = _localizer.Get(isPayloadTooLarge
                ? "payload_too_large_title"
                : "invalid_request_title"),
            TraceId = GetTraceId(httpContext),
            Type = isPayloadTooLarge ? PayloadTooLargeType : BadRequestType
        };

        await httpContext.Response.WriteAsJsonAsync(
            problemDetails,
            options: null,
            contentType: ProblemJsonContentType,
            cancellationToken);
    }

    private static string GetTraceId(HttpContext httpContext) =>
        Activity.Current?.Id ?? httpContext.TraceIdentifier;
}
