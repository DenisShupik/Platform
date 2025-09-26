using Shared.Application.Interfaces;
using Shared.Domain.Abstractions;
using UserService.Application.Interfaces;
using UserService.Domain.ValueObjects;

namespace UserService.Application.UseCases;

public sealed class GetUsersBulkQuery<T> : IQuery<IReadOnlyList<T>>
{
    /// <summary>
    /// Идентификаторы пользователей
    /// </summary>
    public required IdSet<UserId, Guid> UserIds { get; init; }
}

public sealed class GetUsersBulkQueryHandler<T> : IQueryHandler<GetUsersBulkQuery<T>, IReadOnlyList<T>>
{
    private readonly IUserReadRepository _repository;

    public GetUsersBulkQueryHandler(IUserReadRepository repository)
    {
        _repository = repository;
    }

    public Task<IReadOnlyList<T>> HandleAsync(GetUsersBulkQuery<T> request, CancellationToken cancellationToken
    )
    {
        return _repository.GetBulkAsync<T>(request.UserIds, cancellationToken);
    }
}