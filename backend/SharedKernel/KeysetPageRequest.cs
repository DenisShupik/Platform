using FluentValidation;
using Microsoft.AspNetCore.Mvc;

namespace SharedKernel;

public abstract class LongKeysetPageRequest
{
    [FromQuery] public long? Cursor { get; set; }
    [FromQuery] public int? PageSize { get; set; }
}

public abstract class LongKeysetPageRequestValidator<T> : AbstractValidator<T>
    where T : LongKeysetPageRequest
{
    public LongKeysetPageRequestValidator()
    {
        RuleFor(e => e.Cursor)
            .GreaterThanOrEqualTo(0)
            .When(e => e.Cursor.HasValue);

        RuleFor(e => e.PageSize)
            .InclusiveBetween(1, 100)
            .When(e => e.PageSize.HasValue);
    }
}

public class KeysetPageResponse<T>
{
    public IReadOnlyList<T> Items { get; set; } = null!;
}