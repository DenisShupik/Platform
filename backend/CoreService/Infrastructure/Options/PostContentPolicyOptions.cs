using FluentValidation;

namespace CoreService.Infrastructure.Options;

public sealed class PostContentPolicyOptions
{
    /// <summary>
    /// Если список пуст, разрешены ссылки на любой хост по HTTPS.
    /// </summary>
    public string[] AllowedLinkHosts { get; set; } = [];
}

public sealed class PostContentPolicyOptionsValidator : AbstractValidator<PostContentPolicyOptions>
{
    public PostContentPolicyOptionsValidator()
    {
        RuleForEach(options => options.AllowedLinkHosts)
            .Must(static host => Uri.CheckHostName(host) == UriHostNameType.Dns)
            .WithMessage("Allowed link host must be a DNS host name.");
    }
}
