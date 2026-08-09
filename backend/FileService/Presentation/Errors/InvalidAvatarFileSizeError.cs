using Shared.Domain.Abstractions.Errors;

namespace FileService.Presentation.Errors;

public sealed record InvalidAvatarFileSizeError(
    long MinimumFileSize,
    long MaximumFileSize,
    long ActualFileSize) : ValidationError;
