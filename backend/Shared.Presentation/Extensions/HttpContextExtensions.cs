using System.Diagnostics.CodeAnalysis;
using System.Runtime.CompilerServices;
using System.Security.Claims;
using System.Text.Json;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.Json;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using Shared.Domain.Enums;
using Shared.Domain.Errors;
using Shared.Domain.Helpers;
using Shared.Domain.Interfaces;
using Shared.Domain.ValueObjects;
using Shared.Presentation.Errors;
using Shared.Presentation.Exceptions;
using Vogen;
using static Shared.Domain.Helpers.ParseExtendedHelper;

namespace Shared.Presentation.Extensions;

public static class HttpContextExtensions
{
    private static string ValidationError(string code, params object[] parameters) =>
        ValidationErrorCodec.Encode(code, parameters);

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private static void AddError(
        ref Dictionary<string, string>? errors,
        string propertyName,
        string error)
    {
        (errors ??= new Dictionary<string, string>(StringComparer.Ordinal)).Add(propertyName, error);
    }

    #region FromRoute

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private static bool GetRouteValueInput(
        this HttpContext context,
        string propertyName,
        ref Dictionary<string, string>? errors,
        [NotNullWhen(true)] out string? input
    )
    {
        if (!context.Request.RouteValues.TryGetValue(propertyName, out var routeValue))
        {
            AddError(ref errors, propertyName, ValidationError(ValidationErrorCodes.RequiredRouteValue));
            input = null;
            return false;
        }

        if (routeValue is not string routeString)
        {
            AddError(ref errors, propertyName, ValidationError(ValidationErrorCodes.InvalidRouteValue));
            input = null;
            return false;
        }

        input = routeString;
        return true;
    }

    public static T TryParseValueObjectFromRoute<T, P>(this HttpContext context, string propertyName,
        ref Dictionary<string, string>? errors)
        where T : struct, IHasTryFrom<T, P>, IVogen<T, P>
        where P : ISpanParsable<P>
    {
        if (!context.GetRouteValueInput(propertyName, ref errors, out var input)) return default;
        if (TryParseExtended<T, P>(input, out var value, out var error)) return value.Value;
        AddError(ref errors, propertyName, error);
        return default;
    }

    public static T TryParseValueTypeFromRoute<T>(this HttpContext context, string propertyName,
        ref Dictionary<string, string>? errors)
        where T : struct, IValueTypeWithTryParseExtended<T>
    {
        if (!context.GetRouteValueInput(propertyName, ref errors, out var input)) return default;
        if (T.TryParseExtended(input, out var result, out var error)) return result.Value;
        AddError(ref errors, propertyName, error);
        return default;
    }

    public static T TryParseReferenceTypeFromRoute<T>(this HttpContext context, string propertyName,
        ref Dictionary<string, string>? errors)
        where T : class, IReferenceTypeWithTryParseExtended<T>
    {
        if (!context.GetRouteValueInput(propertyName, ref errors, out var input)) return T.Default;
        if (T.TryParseExtended(input, out var result, out var error)) return result;
        AddError(ref errors, propertyName, error);
        return T.Default;
    }

    public static T TryParseEnumFromRoute<T>(this HttpContext context, string propertyName,
        ref Dictionary<string, string>? errors)
        where T : struct, Enum
    {
        if (!context.GetRouteValueInput(propertyName, ref errors, out var input)) return default;
        if (!Enum.TryParse<T>(input, true, out var result))
        {
            AddError(ref errors, propertyName, ValidationError(ValidationErrorCodes.CannotParseInputValue));
            return default;
        }

        return result;
    }

    public static T TryParseRequiredPrimitiveFromRoute<T>(this HttpContext context, string propertyName,
        ref Dictionary<string, string>? errors)
        where T : struct, IParsable<T>
    {
        if (!context.GetRouteValueInput(propertyName, ref errors, out var input)) return default;
        if (!T.TryParse(input, null, out var result))
        {
            AddError(ref errors, propertyName, ValidationError(ValidationErrorCodes.CannotParseInputValue));
            return default;
        }

        return result;
    }

    #endregion

    #region FromQuery

    #region ValueObjectFromQuery

    public static T TryParseRequiredValueObjectFromQuery<T, P>(this HttpContext context, string propertyName,
        ref Dictionary<string, string>? errors)
        where T : struct, IHasTryFrom<T, P>, IVogen<T, P>
        where P : ISpanParsable<P>
    {
        if (!context.Request.Query.TryGetValue(propertyName, out var input))
        {
            AddError(ref errors, propertyName, ValidationError(ValidationErrorCodes.RequiredQueryValue));
            return default;
        }

        if (TryParseExtended<T, P>(input, out var value, out var error)) return value.Value;
        AddError(ref errors, propertyName, error);
        return default;
    }

    public static T TryParseDefaultableValueObjectFromQuery<T, P>(this HttpContext context, string propertyName,
        ref Dictionary<string, string>? errors, T defaultValue)
        where T : struct, IHasTryFrom<T, P>, IVogen<T, P>
        where P : ISpanParsable<P>
    {
        if (!context.Request.Query.TryGetValue(propertyName, out var input)) return defaultValue;
        if (TryParseExtended<T, P>(input, out var value, out var error)) return value.Value;
        AddError(ref errors, propertyName, error);
        return defaultValue;
    }

    public static T? TryParseOptionalValueObjectFromQuery<T, P>(this HttpContext context, string propertyName,
        ref Dictionary<string, string>? errors)
        where T : struct, IHasTryFrom<T, P>, IVogen<T, P>
        where P : ISpanParsable<P>
    {
        if (!context.Request.Query.TryGetValue(propertyName, out var input)) return null;
        if (TryParseExtended<T, P>(input, out var value, out var error)) return value;
        AddError(ref errors, propertyName, error);
        return null;
    }

    #endregion

    #region ValueTypeFromQuery

    public static T TryParseRequiredValueTypeFromQuery<T>(this HttpContext context, string propertyName,
        ref Dictionary<string, string>? errors)
        where T : struct, IValueTypeWithTryParseExtended<T>
    {
        if (!context.Request.Query.TryGetValue(propertyName, out var input))
        {
            AddError(ref errors, propertyName, ValidationError(ValidationErrorCodes.RequiredQueryValue));
            return default;
        }

        if (T.TryParseExtended(input, out var result, out var error)) return result.Value;
        AddError(ref errors, propertyName, error);
        return default;
    }

    public static T TryParseDefaultableValueTypeFromQuery<T>(this HttpContext context, string propertyName,
        ref Dictionary<string, string>? errors, T defaultValue)
        where T : struct, IValueTypeWithTryParseExtended<T>
    {
        if (!context.Request.Query.TryGetValue(propertyName, out var input)) return defaultValue;
        if (T.TryParseExtended(input, out var result, out var error)) return result.Value;
        AddError(ref errors, propertyName, error);
        return defaultValue;
    }

    public static T? TryParseOptionalValueTypeFromQuery<T>(this HttpContext context, string propertyName,
        ref Dictionary<string, string>? errors)
        where T : struct, IValueTypeWithTryParseExtended<T>
    {
        if (!context.Request.Query.TryGetValue(propertyName, out var input)) return null;
        if (T.TryParseExtended(input, out var result, out var error)) return result.Value;
        AddError(ref errors, propertyName, error);
        return null;
    }

    #endregion

    #region ReferenceTypeFromQuery

    public static T TryParseRequiredReferenceTypeFromQuery<T>(this HttpContext context, string propertyName,
        ref Dictionary<string, string>? errors)
        where T : class, IReferenceTypeWithTryParseExtended<T>
    {
        if (!context.Request.Query.TryGetValue(propertyName, out var input))
        {
            AddError(ref errors, propertyName, ValidationError(ValidationErrorCodes.RequiredQueryValue));
            return T.Default;
        }

        if (T.TryParseExtended(input, out var result, out var error)) return result;
        AddError(ref errors, propertyName, error);
        return T.Default;
    }

    public static T TryParseDefaultableReferenceTypeFromQuery<T>(this HttpContext context, string propertyName,
        ref Dictionary<string, string>? errors, T defaultValue)
        where T : class, IReferenceTypeWithTryParseExtended<T>
    {
        if (!context.Request.Query.TryGetValue(propertyName, out var input)) return defaultValue;
        if (T.TryParseExtended(input, out var result, out var error)) return result;
        AddError(ref errors, propertyName, error);
        return defaultValue;
    }

    public static T? TryParseOptionalReferenceTypeFromQuery<T>(this HttpContext context, string propertyName,
        ref Dictionary<string, string>? errors)
        where T : class, IReferenceTypeWithTryParseExtended<T>
    {
        if (!context.Request.Query.TryGetValue(propertyName, out var input)) return null;
        if (T.TryParseExtended(input, out var result, out var error)) return result;
        AddError(ref errors, propertyName, error);
        return null;
    }

    #endregion

    #region EnumFromQuery

    public static T TryParseRequiredEnumFromQuery<T>(this HttpContext context, string propertyName,
        ref Dictionary<string, string>? errors)
        where T : struct, Enum
    {
        if (!context.Request.Query.TryGetValue(propertyName, out var input))
        {
            AddError(ref errors, propertyName, ValidationError(ValidationErrorCodes.RequiredQueryValue));
            return default;
        }

        if (TryParseExtended<T>(input, out var result, out var error)) return result.Value;
        AddError(ref errors, propertyName, error);
        return default;
    }

    public static T TryParseDefaultableEnumFromQuery<T>(this HttpContext context, string propertyName,
        ref Dictionary<string, string>? errors, T defaultValue)
        where T : struct, Enum
    {
        if (!context.Request.Query.TryGetValue(propertyName, out var input)) return defaultValue;
        if (TryParseExtended<T>(input, out var result, out var error)) return result.Value;
        AddError(ref errors, propertyName, error);
        return defaultValue;
    }

    public static T? TryParseOptionalEnumFromQuery<T>(this HttpContext context, string propertyName,
        ref Dictionary<string, string>? errors)
        where T : struct, Enum
    {
        if (!context.Request.Query.TryGetValue(propertyName, out var input)) return null;
        if (TryParseExtended<T>(input, out var result, out var error)) return result.Value;
        AddError(ref errors, propertyName, error);
        return null;
    }

    #endregion

    #region PrimitiveFromQuery

    public static T TryParseRequiredPrimitiveFromQuery<T>(this HttpContext context, string propertyName,
        ref Dictionary<string, string>? errors)
        where T : struct, IParsable<T>
    {
        if (!context.Request.Query.TryGetValue(propertyName, out var input))
        {
            AddError(ref errors, propertyName, ValidationError(ValidationErrorCodes.RequiredQueryValue));
            return default;
        }

        if (!T.TryParse(input, null, out var result))
        {
            AddError(ref errors, propertyName, ValidationError(ValidationErrorCodes.CannotParseInputValue));
            return default;
        }

        return result;
    }

    public static T TryParseDefaultablePrimitiveFromQuery<T>(this HttpContext context, string propertyName,
        ref Dictionary<string, string>? errors, T defaultValue)
        where T : struct, IParsable<T>
    {
        if (!context.Request.Query.TryGetValue(propertyName, out var input)) return defaultValue;

        if (!T.TryParse(input, null, out var result))
        {
            AddError(ref errors, propertyName, ValidationError(ValidationErrorCodes.CannotParseInputValue));
            return defaultValue;
        }

        return result;
    }

    public static T? TryParseOptionalPrimitiveFromQuery<T>(this HttpContext context, string propertyName,
        ref Dictionary<string, string>? errors)
        where T : struct, IParsable<T>
    {
        if (!context.Request.Query.TryGetValue(propertyName, out var input)) return null;

        if (!T.TryParse(input, null, out var result))
        {
            AddError(ref errors, propertyName, ValidationError(ValidationErrorCodes.CannotParseInputValue));
            return null;
        }

        return result;
    }

    #endregion

    #endregion

    public const string BodyPropertyName = "body";

    public static ValueTask<T?> ReadBodyFromJsonAsync<T>(this HttpContext context) where T : notnull
    {
        var jsonOptions = context.RequestServices.GetRequiredService<IOptions<JsonOptions>>().Value;
        return context.Request.ReadFromJsonAsync<T>(
            jsonOptions.SerializerOptions,
            context.RequestAborted);
    }

    public static void AddJsonBodyError(ref Dictionary<string, string>? errors, JsonException exception)
    {
        var error = exception.InnerException is ValueObjectValidationException valueObjectValidationException
            ? valueObjectValidationException.Message
            : ValidationError(ValidationErrorCodes.InvalidJsonBody);
        AddError(ref errors,
            exception.Path == null ? BodyPropertyName : $"{BodyPropertyName}({exception.Path})",
            error);
    }

    public static void AddInvalidJsonBodyError(ref Dictionary<string, string>? errors) =>
        AddError(ref errors, BodyPropertyName, ValidationError(ValidationErrorCodes.InvalidJsonBody));

    public static UserIdRole GetRequiredUserIdRole(this HttpContext context)
    {
        var user = context.User;
        if (user.Identity?.IsAuthenticated != true) throw new UnauthorizedException(new AuthenticationRequiredError());
        return GetUserIdRole(user);
    }

    private static UserIdRole GetUserIdRole(ClaimsPrincipal user)
    {
        string? subClaim = null;
        Role? role = null;

        foreach (var claim in user.Claims)
        {
            if (claim.Type == ClaimTypes.NameIdentifier)
            {
                subClaim ??= claim.Value;
                continue;
            }

            if (claim.Type != ClaimTypes.Role || !Enum.TryParse<Role>(claim.Value, out var parsedRole)) continue;
            if (role == null || parsedRole > role) role = parsedRole;
        }

        if (subClaim == null) throw new UnauthorizedException(new ClaimNotFoundError(ClaimTypes.NameIdentifier));
        if (role == null) throw new UnauthorizedException(new ClaimNotFoundError(ClaimTypes.Role));
        return new UserIdRole(UserId.Parse(subClaim), role.Value);
    }

    public static UserId GetRequiredUserId(this HttpContext context)
    {
        var user = context.User;
        if (user.Identity?.IsAuthenticated != true)
            throw new UnauthorizedException(new AuthenticationRequiredError());

        var subClaim = user.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        if (subClaim == null)
            throw new UnauthorizedException(new ClaimNotFoundError(ClaimTypes.NameIdentifier));

        return UserId.Parse(subClaim);
    }

    public static UserIdRole? GetOptionalUserIdRole(this HttpContext context)
    {
        var user = context.User;
        if (user.Identity?.IsAuthenticated != true) return null;
        return GetUserIdRole(user);
    }
}
