using FluentValidation;

namespace Shared.Infrastructure.Options;

public sealed class KeycloakOptions
{
    public required string MetadataAddress { get; init; }
    public required string Issuer { get; init; }
    public required string Audience { get; init; }
    public required string InternalAudience { get; init; }
    public required string Realm { get; init; }
}

public sealed class KeycloakOptionsValidator : AbstractValidator<KeycloakOptions>
{
    public KeycloakOptionsValidator()
    {
        RuleFor(x => x.MetadataAddress)
            .NotEmpty();

        RuleFor(x => x.Issuer)
            .NotEmpty();

        RuleFor(x => x.Audience)
            .NotEmpty();

        RuleFor(x => x.InternalAudience)
            .NotEmpty();

        RuleFor(x => x.Realm)
            .NotEmpty();

    }
}
