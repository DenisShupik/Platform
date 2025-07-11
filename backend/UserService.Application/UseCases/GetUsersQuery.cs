using SharedKernel.Application.Abstractions;
using UserService.Application.Dtos;
using UserService.Application.Interfaces;

namespace UserService.Application.UseCases;

public sealed class GetUsersQuery : PaginatedQuery;

public sealed class GetUsersQueryHandler
{
    private readonly IUserReadRepository _repository;

    public GetUsersQueryHandler(IUserReadRepository repository)
    {
        _repository = repository;
    }

    private Task<IReadOnlyList<T>> HandleAsync<T>(GetUsersQuery request, CancellationToken cancellationToken)
    {
        return _repository.GetAllAsync<T>(request, cancellationToken);
    }

    public async Task<IReadOnlyList<UserDto>> HandleAsync(GetUsersQuery request, CancellationToken cancellationToken)
    {
        return await HandleAsync<UserDto>(request, cancellationToken);
    }
}