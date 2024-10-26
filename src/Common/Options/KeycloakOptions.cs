using FluentValidation;

namespace Common.Options;

public sealed class KeycloakOptions
{
    public string Host { get; set; }
    public string[] Issuers { get; set; }
}

public sealed class KeycloakOptionsValidator : AbstractValidator<KeycloakOptions>
{
    public KeycloakOptionsValidator()
    {
        RuleFor(options => options.Host)
            .NotEmpty();
        
        RuleForEach(options => options.Issuers)
            .NotEmpty();
    }
}