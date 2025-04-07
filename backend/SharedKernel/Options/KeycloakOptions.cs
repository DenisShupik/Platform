using FluentValidation;

namespace SharedKernel.Options;

public sealed class KeycloakOptions
{
    public string MetadataAddress { get; set; }
    public string Issuer { get; set; }
    public string Audience { get; set; }
    public string Realm { get; set; }
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
    }
}