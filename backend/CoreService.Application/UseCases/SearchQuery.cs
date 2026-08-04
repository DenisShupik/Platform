using CoreService.Application.Dtos;
using CoreService.Application.Interfaces;
using CoreService.Domain.Errors;
using CoreService.Domain.ValueObjects;
using Shared.Application.Abstractions;
using Shared.Application.Interfaces;
using Shared.Domain.Abstractions.Results;
using Shared.Domain.ValueObjects;

namespace CoreService.Application.UseCases;

public enum SearchQuerySortType : byte
{
    Relevance = 0,
    Newest = 1
}

public sealed class SearchQuery : SingleSortPagedQuery<
    Result<SearchResultsDto, InvalidSearchCursorError>,
    SearchQuerySortType>
{
    public required SearchTerm Term { get; init; }
    public required SearchResultType? Type { get; init; }
    public required SearchCursor? Cursor { get; init; }
    public required UserIdRole? QueriedBy { get; init; }
}

public sealed class SearchQueryHandler : IQueryHandler<
    SearchQuery,
    Result<SearchResultsDto, InvalidSearchCursorError>>
{
    private readonly ISearchReadRepository _repository;

    public SearchQueryHandler(ISearchReadRepository repository)
    {
        _repository = repository;
    }

    public Task<Result<SearchResultsDto, InvalidSearchCursorError>> HandleAsync(
        SearchQuery query,
        CancellationToken cancellationToken) =>
        _repository.SearchAsync(query, cancellationToken);
}
