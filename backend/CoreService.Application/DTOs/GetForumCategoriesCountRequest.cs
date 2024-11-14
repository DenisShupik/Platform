using FluentValidation;
using Microsoft.AspNetCore.Mvc;
using SharedKernel.Batching;

namespace CoreService.Application.DTOs;

public sealed class GetForumCategoriesCountRequest
{
    [FromRoute] public LongIds ForumIds { get; set; }
}

public sealed class GetForumCategoriesCountRequestValidator : AbstractValidator<GetForumCategoriesCountRequest>
{
    public GetForumCategoriesCountRequestValidator()
    {
        RuleForEach(e => e.ForumIds)
            .GreaterThan(0);
    }
}