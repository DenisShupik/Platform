using SharedKernel.Application.Abstractions; 
using UserService.Application.Dtos;
using UserService.Application.Interfaces;
using UserService.Domain.ValueObjects;

namespace UserService.Application.UseCases;

public sealed class GetUsersBulkQuery
{
    /// <summary>
    /// Идентификаторы пользователей
    /// </summary>
    public required IdSet<UserId> UserIds { get; init; }
}

public sealed class GetUsersBulkQueryHandler
{
    private readonly IUserReadRepository _repository;

    public GetUsersBulkQueryHandler(IUserReadRepository repository)
    {
        _repository = repository;
    }

    private Task<IReadOnlyList<T>> HandleAsync<T>(
        GetUsersBulkQuery request, CancellationToken cancellationToken
    )
    {
        return _repository.GetBulkAsync<T>(request.UserIds, cancellationToken);
    }

    public Task<IReadOnlyList<UserDto>> HandleAsync(
        GetUsersBulkQuery query, CancellationToken cancellationToken
    )
    {
        return HandleAsync<UserDto>(query, cancellationToken);
    }
}