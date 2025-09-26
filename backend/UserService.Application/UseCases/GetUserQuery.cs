using Shared.TypeGenerator.Attributes;
using OneOf;
using Shared.Application.Interfaces;
using UserService.Application.Interfaces;
using UserService.Domain.Entities;
using UserService.Domain.Errors;

namespace UserService.Application.UseCases;

[Include(typeof(User), PropertyGenerationMode.AsRequired, nameof(User.UserId))]
public sealed partial class GetUserQuery<T> : IQuery<OneOf<T, UserNotFoundError>>;

public sealed class GetUserQueryHandler<T> : IQueryHandler<GetUserQuery<T>, OneOf<T, UserNotFoundError>>
{
    private readonly IUserReadRepository _repository;

    public GetUserQueryHandler(IUserReadRepository repository)
    {
        _repository = repository;
    }

    public Task<OneOf<T, UserNotFoundError>> HandleAsync(
        GetUserQuery<T> query, CancellationToken cancellationToken
    )
    {
        return _repository.GetOneAsync<T>(query.UserId, cancellationToken);
    }
}