using FluentValidation;

namespace Shared.Infrastructure.Options;

public sealed class KeycloakOptions
{
    public required string MetadataAddress { get; init; }
    public required string Issuer { get; init; }
    public required string Audience { get; init; }
    public required string Realm { get; init; }
    public required string ServiceClientId { get; init; }
    public required string ServiceClientSecret { get; init; }
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

        RuleFor(x => x.Realm)
            .NotEmpty();
        
        RuleFor(x => x.ServiceClientId)
            .NotEmpty();
        
        RuleFor(x => x.ServiceClientSecret)
            .NotEmpty();
    }
}