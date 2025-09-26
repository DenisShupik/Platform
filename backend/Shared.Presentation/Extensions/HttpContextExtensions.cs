using System.ComponentModel.DataAnnotations;
using System.Diagnostics.CodeAnalysis;
using System.Runtime.CompilerServices;
using System.Text.Json;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.Json;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using Shared.Application.Interfaces;
using Shared.Domain.Interfaces;
using static Shared.Domain.Helpers.ParseExtendedHelper;

namespace Shared.Presentation.Extensions;

public static class HttpContextExtensions
{
    private const string ErrorMessage = "Invalid body";

    #region FromRoute

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private static bool GetRouteValueInput(
        this HttpContext context,
        string propertyName,
        Dictionary<string, string> errors,
        [NotNullWhen(true)] out string? input
    )
    {
        if (!context.Request.RouteValues.TryGetValue(propertyName, out var routeValue))
        {
            errors.Add(propertyName, "route value is required");
            input = null;
            return false;
        }

        if (routeValue is not string routeString)
        {
            errors.Add(propertyName, "invalid route value");
            input = null;
            return false;
        }

        input = routeString;
        return true;
    }

    public static T TryParseValueObjectFromRoute<T, P>(this HttpContext context, string propertyName,
        Dictionary<string, string> errors)
        where T : struct, IHasTryFrom<T, P>, IVogen<T, P>
        where P : ISpanParsable<P>
    {
        if (!context.GetRouteValueInput(propertyName, errors, out var input)) return default;
        if (TryParseExtended<T, P>(input, out var value, out var error)) return value.Value;
        errors.Add(propertyName, error);
        return default;
    }

    public static T TryParseValueTypeFromRoute<T>(this HttpContext context, string propertyName,
        Dictionary<string, string> errors)
        where T : struct, IValueTypeWithTryParseExtended<T>
    {
        if (!context.GetRouteValueInput(propertyName, errors, out var input)) return default;
        if (T.TryParseExtended(input, out var result, out var error)) return result.Value;
        errors.Add(propertyName, error);
        return default;
    }

    public static T TryParseReferenceTypeFromRoute<T>(this HttpContext context, string propertyName,
        Dictionary<string, string> errors)
        where T : class, IReferenceTypeWithTryParseExtended<T>
    {
        if (!context.GetRouteValueInput(propertyName, errors, out var input)) return T.Default;
        if (T.TryParseExtended(input, out var result, out var error)) return result;
        errors.Add(propertyName, error);
        return T.Default;
    }

    public static T TryParseRequiredEnumFromRoute<T>(this HttpContext context, string propertyName,
        Dictionary<string, string> errors)
        where T : struct, Enum
    {
        if (!context.GetRouteValueInput(propertyName, errors, out var input)) return default;

        T result;
        try
        {
            result = Enum.Parse<T>(input, true);
        }
        catch
        {
            errors.Add(propertyName, "Cannot parse input value");
            return default;
        }

        return result;
    }

    public static T TryParseRequiredPrimitiveFromRoute<T>(this HttpContext context, string propertyName,
        Dictionary<string, string> errors)
        where T : struct, IParsable<T>
    {
        if (!context.GetRouteValueInput(propertyName, errors, out var input)) return default;

        T result;
        try
        {
            result = T.Parse(input, null);
        }
        catch
        {
            errors.Add(propertyName, "Cannot parse input value");
            return default;
        }

        return result;
    }

    #endregion

    #region FromQuery

    #region ValueObjectFromQuery

    public static T TryParseRequiredValueObjectFromQuery<T, P>(this HttpContext context, string propertyName,
        Dictionary<string, string> errors)
        where T : struct, IHasTryFrom<T, P>, IVogen<T, P>
        where P : ISpanParsable<P>
    {
        if (!context.Request.Query.TryGetValue(propertyName, out var input))
        {
            errors.Add(propertyName, "query value is required");
            return default;
        }

        if (TryParseExtended<T, P>(input, out var value, out var error)) return value.Value;
        errors.Add(propertyName, error);
        return default;
    }

    public static T TryParseDefaultableValueObjectFromQuery<T, P>(this HttpContext context, string propertyName,
        Dictionary<string, string> errors, T defaultValue)
        where T : struct, IHasTryFrom<T, P>, IVogen<T, P>
        where P : ISpanParsable<P>
    {
        if (!context.Request.Query.TryGetValue(propertyName, out var input)) return defaultValue;
        if (TryParseExtended<T, P>(input, out var value, out var error)) return value.Value;
        errors.Add(propertyName, error);
        return defaultValue;
    }

    public static T? TryParseOptionalValueObjectFromQuery<T, P>(this HttpContext context, string propertyName,
        Dictionary<string, string> errors)
        where T : struct, IHasTryFrom<T, P>, IVogen<T, P>
        where P : ISpanParsable<P>
    {
        if (!context.Request.Query.TryGetValue(propertyName, out var input)) return null;
        if (TryParseExtended<T, P>(input, out var value, out var error)) return value;
        errors.Add(propertyName, error);
        return null;
    }

    #endregion

    #region ValueTypeFromQuery

    public static T TryParseRequiredValueTypeFromQuery<T>(this HttpContext context, string propertyName,
        Dictionary<string, string> errors)
        where T : struct, IValueTypeWithTryParseExtended<T>
    {
        if (!context.Request.Query.TryGetValue(propertyName, out var input))
        {
            errors.Add(propertyName, "query value is required");
            return default;
        }

        if (T.TryParseExtended(input, out var result, out var error)) return result.Value;
        errors.Add(propertyName, error);
        return default;
    }

    public static T TryParseDefaultableValueTypeFromQuery<T>(this HttpContext context, string propertyName,
        Dictionary<string, string> errors, T defaultValue)
        where T : struct, IValueTypeWithTryParseExtended<T>
    {
        if (!context.Request.Query.TryGetValue(propertyName, out var input)) return defaultValue;
        if (T.TryParseExtended(input, out var result, out var error)) return result.Value;
        errors.Add(propertyName, error);
        return defaultValue;
    }

    public static T? TryParseOptionalValueTypeFromQuery<T>(this HttpContext context, string propertyName,
        Dictionary<string, string> errors)
        where T : struct, IValueTypeWithTryParseExtended<T>
    {
        if (!context.Request.Query.TryGetValue(propertyName, out var input)) return null;
        if (T.TryParseExtended(input, out var result, out var error)) return result.Value;
        errors.Add(propertyName, error);
        return null;
    }

    #endregion

    #region ReferenceTypeFromQuery

    public static T TryParseRequiredReferenceTypeFromQuery<T>(this HttpContext context, string propertyName,
        Dictionary<string, string> errors)
        where T : class, IReferenceTypeWithTryParseExtended<T>
    {
        if (!context.Request.Query.TryGetValue(propertyName, out var input))
        {
            errors.Add(propertyName, "query value is required");
            return T.Default;
        }

        if (T.TryParseExtended(input, out var result, out var error)) return result;
        errors.Add(propertyName, error);
        return T.Default;
    }

    public static T TryParseDefaultableReferenceTypeFromQuery<T>(this HttpContext context, string propertyName,
        Dictionary<string, string> errors, T defaultValue)
        where T : class, IReferenceTypeWithTryParseExtended<T>
    {
        if (!context.Request.Query.TryGetValue(propertyName, out var input)) return defaultValue;
        if (T.TryParseExtended(input, out var result, out var error)) return result;
        errors.Add(propertyName, error);
        return defaultValue;
    }

    public static T? TryParseOptionalReferenceTypeFromQuery<T>(this HttpContext context, string propertyName,
        Dictionary<string, string> errors)
        where T : class, IReferenceTypeWithTryParseExtended<T>
    {
        if (!context.Request.Query.TryGetValue(propertyName, out var input)) return null;
        if (T.TryParseExtended(input, out var result, out var error)) return result;
        errors.Add(propertyName, error);
        return null;
    }

    #endregion

    #region EnumFromQuery

    public static T TryParseRequiredEnumFromQuery<T>(this HttpContext context, string propertyName,
        Dictionary<string, string> errors)
        where T : struct, Enum
    {
        if (!context.Request.Query.TryGetValue(propertyName, out var input))
        {
            errors.Add(propertyName, "query value is required");
            return default;
        }

        if (TryParseExtended<T>(input, out var result, out var error)) return result.Value;
        errors.Add(propertyName, error);
        return default;
    }

    public static T TryParseDefaultableEnumFromQuery<T>(this HttpContext context, string propertyName,
        Dictionary<string, string> errors, T defaultValue)
        where T : struct, Enum
    {
        if (!context.Request.Query.TryGetValue(propertyName, out var input)) return defaultValue;
        if (TryParseExtended<T>(input, out var result, out var error)) return result.Value;
        errors.Add(propertyName, error);
        return defaultValue;
    }

    public static T? TryParseOptionalEnumFromQuery<T>(this HttpContext context, string propertyName,
        Dictionary<string, string> errors)
        where T : struct, Enum
    {
        if (!context.Request.Query.TryGetValue(propertyName, out var input)) return null;
        if (TryParseExtended<T>(input, out var result, out var error)) return result.Value;
        errors.Add(propertyName, error);
        return null;
    }

    #endregion

    #region PrimitiveFromQuery

    public static T TryParseRequiredPrimitiveFromQuery<T>(this HttpContext context, string propertyName,
        Dictionary<string, string> errors)
        where T : struct, IParsable<T>
    {
        if (!context.Request.Query.TryGetValue(propertyName, out var input))
        {
            errors.Add(propertyName, "query value is required");
            return default;
        }

        T result;
        try
        {
            result = T.Parse(input, null);
        }
        catch
        {
            errors.Add(propertyName, "Cannot parse input value");
            return default;
        }

        return result;
    }

    public static T TryParseDefaultablePrimitiveFromQuery<T>(this HttpContext context, string propertyName,
        Dictionary<string, string> errors, T defaultValue)
        where T : struct, IParsable<T>
    {
        if (!context.Request.Query.TryGetValue(propertyName, out var input)) return defaultValue;

        T result;
        try
        {
            result = T.Parse(input, null);
        }
        catch
        {
            errors.Add(propertyName, "Cannot parse input value");
            return defaultValue;
        }

        return result;
    }

    public static T? TryParseOptionalPrimitiveFromQuery<T>(this HttpContext context, string propertyName,
        Dictionary<string, string> errors)
        where T : struct, IParsable<T>
    {
        if (!context.Request.Query.TryGetValue(propertyName, out var input)) return null;

        T result;
        try
        {
            result = T.Parse(input, null);
        }
        catch
        {
            errors.Add(propertyName, "Cannot parse input value");
            return null;
        }

        return result;
    }

    #endregion

    #endregion

    public const string BodyPropertyName = "body";

    public static async ValueTask<T?> GetRequiredBodyFromJsonAsync<T>(this HttpContext context,
        Dictionary<string, string> errors) where T : notnull
    {
        var jsonOptions = context.RequestServices.GetService<IOptions<JsonOptions>>()?.Value;
        T? maybeBody;
        try
        {
            maybeBody = await context.Request.ReadFromJsonAsync<T>(
                jsonOptions?.SerializerOptions,
                context.RequestAborted);
        }
        catch (JsonException exception)
        {
            errors.Add(exception.Path == null ? BodyPropertyName : $"{BodyPropertyName}({exception.Path})",
                exception.Message);
            return default;
        }
        catch
        {
            errors.Add(BodyPropertyName, ErrorMessage);
            return default;
        }

        if (maybeBody == null)
        {
            errors.Add(BodyPropertyName, ErrorMessage);
            return default;
        }

        return maybeBody;
    }
}