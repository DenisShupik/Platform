using FluentValidation;

namespace SharedKernel.Application.Abstractions;

public abstract class PagedQuery
{
    public required int Offset { get; init; }
    public required int Limit { get; init; }
}

public abstract class PagedQueryValidator<TRequest> : AbstractValidator<TRequest>
    where TRequest : PagedQuery
{
    protected PagedQueryValidator()
    {
        RuleFor(x => x.Offset)
            .GreaterThanOrEqualTo(0);

        RuleFor(x => x.Limit)
            .GreaterThan(0);
    }
}