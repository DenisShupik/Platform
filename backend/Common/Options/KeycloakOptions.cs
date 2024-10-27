using FluentValidation;

namespace Common.Options;

public sealed class KeycloakOptions
{
    public string MetadataAddress { get; set; }
    public string Issuer { get; set; }
    public string Audience { get; set; }
}

public sealed class KeycloakOptionsValidator : AbstractValidator<KeycloakOptions>
{
    public KeycloakOptionsValidator()
    {
        RuleFor(options => options.MetadataAddress)
            .NotEmpty();
        
        RuleForEach(options => options.Issuer)
            .NotEmpty();
    }
}