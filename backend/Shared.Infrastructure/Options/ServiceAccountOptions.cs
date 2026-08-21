using FluentValidation;

namespace Shared.Infrastructure.Options;

/// <summary>
/// Credentials of the current workload for outbound internal calls.
/// </summary>
public sealed class ServiceAccountOptions
{
    public required string ClientId { get; init; }
    public required string ClientSecret { get; init; }
}

public sealed class ServiceAccountOptionsValidator : AbstractValidator<ServiceAccountOptions>
{
    public ServiceAccountOptionsValidator()
    {
        RuleFor(options => options.ClientId).NotEmpty();
        RuleFor(options => options.ClientSecret).NotEmpty();
    }
}
