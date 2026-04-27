using Shared.Application.Abstractions;
using Shared.Application.Interfaces;
using UserService.Application.Interfaces;

namespace UserService.Application.UseCases;

public enum GetUsersPagedQuerySortType : byte
{
    UserId = 0
}

public sealed class GetUsersPagedQuery<T> : SingleSortPagedQuery<IReadOnlyList<T>, GetUsersPagedQuerySortType>;

public sealed class GetUsersPagedQueryHandler<T> : IQueryHandler<GetUsersPagedQuery<T>, IReadOnlyList<T>>
{
    private readonly IUserReadRepository _repository;

    public GetUsersPagedQueryHandler(IUserReadRepository repository)
    {
        _repository = repository;
    }

    public Task<IReadOnlyList<T>> HandleAsync(GetUsersPagedQuery<T> query, CancellationToken cancellationToken)
    {
        return _repository.GetAllAsync(query, cancellationToken);
    }
}