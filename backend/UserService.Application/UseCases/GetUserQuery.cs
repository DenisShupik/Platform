using SharedKernel.TypeGenerator;
using OneOf;
using UserService.Application.Dtos;
using UserService.Application.Interfaces;
using UserService.Domain.Entities;
using UserService.Domain.Errors;

namespace UserService.Application.UseCases;

[Include(typeof(User), PropertyGenerationMode.AsRequired, nameof(User.UserId))]
public sealed partial class GetUserQuery;

public sealed class GetUserQueryHandler
{
    private readonly IUserReadRepository _repository;

    public GetUserQueryHandler(IUserReadRepository repository)
    {
        _repository = repository;
    }

    private Task<OneOf<T, UserNotFoundError>> HandleAsync<T>(
        GetUserQuery request, CancellationToken cancellationToken
    )
    {
        return _repository.GetOneAsync<T>(request.UserId, cancellationToken);
    }

    public Task<OneOf<UserDto, UserNotFoundError>> HandleAsync(
        GetUserQuery query, CancellationToken cancellationToken
    )
    {
        return HandleAsync<UserDto>(query, cancellationToken);
    }
}