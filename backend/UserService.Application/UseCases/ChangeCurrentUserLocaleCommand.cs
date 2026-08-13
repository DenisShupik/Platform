using Shared.Application.Interfaces;
using Shared.Domain.Abstractions.Results;
using Shared.Domain.Errors;
using Shared.Domain.ValueObjects;
using UserService.Application.Interfaces;

namespace UserService.Application.UseCases;

public sealed class ChangeCurrentUserLocaleCommand :
    ICommand<SuccessOr<UserNotFoundError>>
{
    public required UserId UserId { get; init; }
    public required Locale Locale { get; init; }
}

public sealed class ChangeCurrentUserLocaleCommandHandler :
    ICommandHandler<ChangeCurrentUserLocaleCommand, SuccessOr<UserNotFoundError>>
{
    private readonly IUserLocaleIdentityProvider _identityProvider;

    public ChangeCurrentUserLocaleCommandHandler(IUserLocaleIdentityProvider identityProvider)
    {
        _identityProvider = identityProvider;
    }

    public async Task<SuccessOr<UserNotFoundError>> HandleAsync(
        ChangeCurrentUserLocaleCommand command,
        CancellationToken cancellationToken)
    {
        var userExists = await _identityProvider.ChangeLocaleAsync(
            command.UserId,
            command.Locale,
            cancellationToken);

        return userExists
            ? SuccessOr.Success
            : new UserNotFoundError();
    }
}
