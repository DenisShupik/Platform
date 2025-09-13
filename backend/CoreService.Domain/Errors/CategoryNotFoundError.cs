using CoreService.Domain.ValueObjects;
using Shared.Domain.Errors;

namespace CoreService.Domain.Errors;

public record CategoryNotFoundError(CategoryId CategoryId) : NotFoundError;