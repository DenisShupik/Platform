using FluentValidation;
using Microsoft.AspNetCore.Mvc;

namespace CoreService.Application.DTOs;

public sealed class GetForumCategoriesCountRequest
{
    [FromRoute] public long ForumId { get; set; }
}

public sealed class GetForumCategoriesCountRequestValidator : AbstractValidator<GetForumCategoriesCountRequest>
{
    public GetForumCategoriesCountRequestValidator()
    {
        RuleFor(e => e.ForumId)
            .GreaterThan(0);
    }
}