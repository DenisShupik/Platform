using Shared.Application.Interfaces;
using Shared.Domain.Abstractions;
using Shared.Domain.Abstractions.Results;
using Shared.Domain.Errors;
using Shared.Domain.ValueObjects;
using UserService.Application.Interfaces;

namespace UserService.Application.UseCases;

public sealed class GetUsersBulkQuery<T> : IQuery<Dictionary<UserId, Result<T, UserNotFoundError>>>
    where T : notnull
{
    /// <summary>
    /// Идентификаторы пользователей
    /// </summary>
    public required IdSet<UserId, Guid> UserIds { get; init; }
}

public sealed class GetUsersBulkQueryHandler<T> : IQueryHandler<GetUsersBulkQuery<T>, Dictionary<UserId, Result<T, UserNotFoundError>>>
    where T : notnull
{
    private readonly IUserReadRepository _repository;

    public GetUsersBulkQueryHandler(IUserReadRepository repository)
    {
        _repository = repository;
    }

    public Task<Dictionary<UserId, Result<T, UserNotFoundError>>> HandleAsync(GetUsersBulkQuery<T> request, CancellationToken cancellationToken
    )
    {
        return _repository.GetBulkAsync<T>(request.UserIds, cancellationToken);
    }
}