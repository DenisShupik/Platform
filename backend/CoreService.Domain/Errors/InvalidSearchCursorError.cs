using Shared.Domain.Abstractions.Errors;

namespace CoreService.Domain.Errors;

/// <summary>
/// Курсор поиска не был сформирован сервером, повреждён либо не относится к текущему запросу.
/// </summary>
public sealed record InvalidSearchCursorError : ValidationError;
