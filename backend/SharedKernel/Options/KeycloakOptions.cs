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
        RuleFor(options => options.MetadataAddress)
            .NotEmpty();

        RuleForEach(options => options.Issuer)
            .NotEmpty();

        RuleForEach(options => options.Audience)
            .NotEmpty();

        RuleForEach(options => options.Realm)
            .NotEmpty();
    }
}