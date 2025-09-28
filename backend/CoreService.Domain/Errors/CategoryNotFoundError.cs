using CoreService.Domain.ValueObjects;
using Shared.Domain.Abstractions.Errors;

namespace CoreService.Domain.Errors;

public record CategoryNotFoundError(CategoryId CategoryId) : NotFoundError;