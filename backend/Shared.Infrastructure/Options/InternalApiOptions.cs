using FluentValidation;

namespace Shared.Infrastructure.Options;

/// <summary>
/// Trusted workload identities. These identify technical accounts, not forum users.
/// </summary>
public sealed class InternalApiOptions
{
    public required string CoreServiceClientId { get; init; }
    public required string NotificationServiceClientId { get; init; }
    public required string ProvisioningServiceClientId { get; init; }
}

public sealed class InternalApiOptionsValidator : AbstractValidator<InternalApiOptions>
{
    public InternalApiOptionsValidator()
    {
        RuleFor(options => options.CoreServiceClientId).NotEmpty();
        RuleFor(options => options.NotificationServiceClientId).NotEmpty();
        RuleFor(options => options.ProvisioningServiceClientId).NotEmpty();
    }
}
