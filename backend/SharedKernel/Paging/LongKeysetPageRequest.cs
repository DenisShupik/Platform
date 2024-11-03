using FluentValidation;
using Microsoft.AspNetCore.Mvc;

namespace SharedKernel.Paging;

public abstract class LongKeysetPageRequest
{
    [FromQuery] public long? Cursor { get; set; }
    [FromQuery] public int? Limit { get; set; }
}

public abstract class LongKeysetPageRequestValidator<T> : AbstractValidator<T>
    where T : LongKeysetPageRequest
{
    public LongKeysetPageRequestValidator()
    {
        RuleFor(e => e.Cursor)
            .GreaterThanOrEqualTo(0)
            .When(e => e.Cursor.HasValue);

        RuleFor(e => e.Limit)
            .InclusiveBetween(1, 100)
            .When(e => e.Limit.HasValue);
    }
}