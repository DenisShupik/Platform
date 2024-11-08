using FluentValidation;
using Microsoft.AspNetCore.Mvc;

namespace CoreService.Application.DTOs;

public sealed class GetForumRequest
{
    /// <summary>
    /// Идентификатор раздела
    /// </summary>
    [FromRoute]
    public long ForumId { get; set; }
}

public sealed class GetForumRequestValidator : AbstractValidator<GetForumRequest>
{
    public GetForumRequestValidator()
    {
        RuleFor(e => e.ForumId)
            .GreaterThan(0);
    }
}