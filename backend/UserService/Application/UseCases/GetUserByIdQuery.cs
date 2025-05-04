using Generator.Attributes;
using OneOf;
using UserService.Application.Dtos;
using UserService.Application.Interfaces;
using UserService.Domain.Entities;
using UserService.Domain.Errors;

namespace UserService.Application.UseCases;

[IncludeAsRequired(typeof(User), nameof(User.UserId))]
public sealed partial class GetUserByIdQuery;

public sealed class GetUserByIdQueryHandler
{
    private readonly IUserReadRepository _repository;

    public GetUserByIdQueryHandler(IUserReadRepository repository)
    {
        _repository = repository;
    }

    private Task<OneOf<T, UserNotFoundError>> HandleAsync<T>(
        GetUserByIdQuery request, CancellationToken cancellationToken
    )
    {
        return _repository.GetByIdAsync<T>(request.UserId, cancellationToken);
    }

    public Task<OneOf<UserDto, UserNotFoundError>> HandleAsync(
        GetUserByIdQuery query, CancellationToken cancellationToken
    )
    {
        return HandleAsync<UserDto>(query, cancellationToken);
    }
}