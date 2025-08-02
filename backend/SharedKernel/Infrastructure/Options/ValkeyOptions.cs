using FluentValidation;

namespace SharedKernel.Infrastructure.Options;

public sealed class ValkeyOptions
{
    public string ConnectionString { get; set; }
}

public sealed class ValkeyOptionsValidator : AbstractValidator<ValkeyOptions>
{
    public ValkeyOptionsValidator()
    {
        RuleFor(options => options.ConnectionString)
            .NotEmpty();
    }
}