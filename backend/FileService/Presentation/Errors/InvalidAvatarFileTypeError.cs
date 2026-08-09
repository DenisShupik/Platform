using Shared.Domain.Abstractions.Errors;

namespace FileService.Presentation.Errors;

public sealed record InvalidAvatarFileTypeError(string ExpectedMediaType) : ValidationError;
