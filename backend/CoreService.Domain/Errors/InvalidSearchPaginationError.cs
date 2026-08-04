using Shared.Domain.Abstractions.Errors;

namespace CoreService.Domain.Errors;

/// <summary>
/// Нельзя одновременно использовать offset и cursor для одной поисковой выдачи.
/// </summary>
public sealed record InvalidSearchPaginationError : ValidationError;
