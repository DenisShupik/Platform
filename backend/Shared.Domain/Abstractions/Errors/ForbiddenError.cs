using System.Text.Json.Serialization;

namespace Shared.Domain.Abstractions.Errors;

[JsonPolymorphic]
public abstract record ForbiddenError : Error;