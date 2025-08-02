using FluentValidation;
using SharedKernel.Infrastructure.Interfaces;

namespace CoreService.Infrastructure.Options;

public sealed class CoreServiceOptions : IDbOptions
{
    public string ConnectionString { get; set; } = null!;
}

public sealed class CoreServiceOptionsValidator : AbstractValidator<CoreServiceOptions>
{
    public CoreServiceOptionsValidator()
    {
        RuleFor(e => e.ConnectionString)
            .NotEmpty();
    }
}