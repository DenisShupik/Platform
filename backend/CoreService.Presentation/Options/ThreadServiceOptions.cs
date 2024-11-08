using FluentValidation;
using SharedKernel.Interfaces;

namespace CoreService.Presentation.Options;

public sealed class ThreadServiceOptions : IDbOptions
{
    public string ConnectionString { get; set; } = null!;
}

public sealed class ThreadServiceOptionsValidator : AbstractValidator<ThreadServiceOptions>
{
    public ThreadServiceOptionsValidator()
    {
        RuleFor(e => e.ConnectionString)
            .NotEmpty();
    }
}