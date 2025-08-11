using SharedKernel.Application.Abstractions;
using UserService.Application.Dtos;
using UserService.Application.Interfaces;

namespace UserService.Application.UseCases;

public sealed class GetUsersPagedQuery : PagedQuery;

public sealed class GetUsersPagedQueryHandler
{
    private readonly IUserReadRepository _repository;

    public GetUsersPagedQueryHandler(IUserReadRepository repository)
    {
        _repository = repository;
    }

    private Task<IReadOnlyList<T>> HandleAsync<T>(GetUsersPagedQuery request, CancellationToken cancellationToken)
    {
        return _repository.GetAllAsync<T>(request, cancellationToken);
    }

    public async Task<IReadOnlyList<UserDto>> HandleAsync(GetUsersPagedQuery request, CancellationToken cancellationToken)
    {
        return await HandleAsync<UserDto>(request, cancellationToken);
    }
}