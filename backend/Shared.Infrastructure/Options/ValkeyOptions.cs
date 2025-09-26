using FluentValidation;

namespace Shared.Infrastructure.Options;

public sealed class ValkeyOptions
{
    public required string ConnectionString { get; init; }
}

public sealed class ValkeyOptionsValidator : AbstractValidator<ValkeyOptions>
{
    public ValkeyOptionsValidator()
    {
        RuleFor(options => options.ConnectionString)
            .NotEmpty();
    }
}