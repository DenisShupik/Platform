using FluentValidation;

namespace Shared.Infrastructure.Options;

public sealed class KeycloakAdminOptions
{
    public required string ClientId { get; init; }
    public required string ClientSecret { get; init; }
}

public sealed class KeycloakAdminOptionsValidator : AbstractValidator<KeycloakAdminOptions>
{
    public KeycloakAdminOptionsValidator()
    {
        RuleFor(x => x.ClientId)
            .NotEmpty();

        RuleFor(x => x.ClientSecret)
            .NotEmpty();
    }
}
