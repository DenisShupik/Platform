using FluentValidation;
using Microsoft.AspNetCore.Mvc;

namespace SharedKernel.Paging;

public abstract class GuidKeysetPageRequest
{
    [FromQuery] public Guid? Cursor { get; set; }
    [FromQuery] public int? Limit { get; set; }
}

public abstract class GuidKeysetPageRequestValidator<T> : AbstractValidator<T>
    where T : GuidKeysetPageRequest
{
    public GuidKeysetPageRequestValidator()
    {
        RuleFor(e => e.Cursor)
            .NotEmpty()
            .When(e => e.Cursor.HasValue);

        RuleFor(e => e.Limit)
            .InclusiveBetween(1, 100)
            .When(e => e.Limit.HasValue);
    }
}