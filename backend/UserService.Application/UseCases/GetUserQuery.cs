using Shared.TypeGenerator.Attributes;
using Shared.Application.Interfaces;
using Shared.Domain.Abstractions;
using Shared.Domain.Abstractions.Results;
using UserService.Application.Interfaces;
using UserService.Domain.Entities;
using UserService.Domain.Errors;

namespace UserService.Application.UseCases;

[Include(typeof(User), PropertyGenerationMode.AsRequired, nameof(User.UserId))]
public sealed partial class GetUserQuery<T> : IQuery<Result<T, UserNotFoundError>> where T : notnull;

public sealed class GetUserQueryHandler<T> : IQueryHandler<GetUserQuery<T>, Result<T, UserNotFoundError>>
    where T : notnull
{
    private readonly IUserReadRepository _repository;

    public GetUserQueryHandler(IUserReadRepository repository)
    {
        _repository = repository;
    }

    public Task<Result<T, UserNotFoundError>> HandleAsync(
        GetUserQuery<T> query, CancellationToken cancellationToken
    )
    {
        return _repository.GetOneAsync<T>(query.UserId, cancellationToken);
    }
}