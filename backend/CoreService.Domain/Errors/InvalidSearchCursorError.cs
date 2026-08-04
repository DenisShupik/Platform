using Shared.Domain.Abstractions.Errors;

namespace CoreService.Domain.Errors;

/// <summary>
/// Курсор поиска не был сформирован сервером либо был повреждён.
/// </summary>
public sealed record InvalidSearchCursorError : ValidationError;
