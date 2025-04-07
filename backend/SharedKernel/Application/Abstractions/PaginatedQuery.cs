using FluentValidation;

namespace SharedKernel.Application.Abstractions;

public abstract class PaginatedQuery
{
    public required int Offset { get; init; }
    public required int Limit { get; init; }
}

public abstract class PaginatedQueryValidator<TRequest> : AbstractValidator<TRequest>
    where TRequest : PaginatedQuery
{
    protected PaginatedQueryValidator()
    {
        RuleFor(x => x.Offset)
            .GreaterThanOrEqualTo(0);

        RuleFor(x => x.Limit)
            .GreaterThan(0);
    }
}