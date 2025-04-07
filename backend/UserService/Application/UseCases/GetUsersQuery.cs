using SharedKernel.Application.Abstractions;
using UserService.Application.Dtos;
using UserService.Application.Interfaces;

namespace UserService.Application.UseCases;

public sealed class GetUsersQuery : PaginatedQuery
{
}

public sealed class GetUsersQueryHandler
{
    private readonly IUserReadRepository _repository;

    public GetUsersQueryHandler(IUserReadRepository repository)
    {
        _repository = repository;
    }

    private Task<IReadOnlyList<T>> HandleAsync<T>(GetUsersQuery query, CancellationToken cancellationToken)
    {
        return _repository.GetAllAsync<T>(query.Offset, query.Limit, cancellationToken);
    }

    public async Task<IReadOnlyList<UserDto>> HandleAsync(GetUsersQuery query, CancellationToken cancellationToken)
    {
        return await HandleAsync<UserDto>(query, cancellationToken);
    }
}