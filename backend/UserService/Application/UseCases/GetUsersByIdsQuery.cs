using FluentValidation;
using SharedKernel.Application.Abstractions;
using SharedKernel.Domain.ValueObjects;
using UserService.Application.Dtos;
using UserService.Application.Interfaces;

namespace UserService.Application.UseCases;

public sealed class GetUsersByIdsQuery
{
    public required IdList<UserId> UserIds { get; init; }
}

public sealed class GetUsersByIdsQueryValidator : AbstractValidator<GetUsersByIdsQuery>
{
    public GetUsersByIdsQueryValidator()
    {
        RuleFor(x => x.UserIds)
            .NotEmpty();
    }
}

public sealed class GetUsersByIdsQueryHandler
{
    private readonly IUserReadRepository _repository;

    public GetUsersByIdsQueryHandler(IUserReadRepository repository)
    {
        _repository = repository;
    }

    private Task<IReadOnlyList<T>> HandleAsync<T>(
        GetUsersByIdsQuery request, CancellationToken cancellationToken
    )
    {
        return _repository.GetByIdsAsync<T>(request.UserIds, cancellationToken);
    }

    public Task<IReadOnlyList<UserDto>> HandleAsync(
        GetUsersByIdsQuery query, CancellationToken cancellationToken
    )
    {
        return HandleAsync<UserDto>(query, cancellationToken);
    }
}