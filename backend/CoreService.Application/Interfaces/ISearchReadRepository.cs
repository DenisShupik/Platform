using CoreService.Application.Dtos;
using CoreService.Application.UseCases;
using CoreService.Domain.Errors;
using Shared.Domain.Abstractions.Results;

namespace CoreService.Application.Interfaces;

public interface ISearchReadRepository
{
    Task<Result<SearchResultsDto, InvalidSearchCursorError, InvalidSearchPaginationError>> SearchAsync(
        SearchQuery query,
        CancellationToken cancellationToken);
}
