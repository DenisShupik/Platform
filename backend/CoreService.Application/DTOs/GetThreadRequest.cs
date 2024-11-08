using FluentValidation;
using Microsoft.AspNetCore.Mvc;

namespace CoreService.Application.DTOs;

public sealed class GetThreadRequest
{
    /// <summary>
    /// Идентификатор темы
    /// </summary>
    [FromRoute]
    public long ThreadId { get; set; }
}

public sealed class GetThreadRequestValidator : AbstractValidator<GetThreadRequest>
{
    public GetThreadRequestValidator()
    {
        RuleFor(e => e.ThreadId)
            .GreaterThan(0);
    }
}