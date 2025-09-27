using System.Text.Json.Serialization;

namespace Shared.Domain.Errors;

[JsonPolymorphic]
public abstract record ForbiddenError : Error;