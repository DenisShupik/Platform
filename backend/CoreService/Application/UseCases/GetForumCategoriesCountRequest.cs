using CoreService.Domain.ValueObjects;
using FluentValidation;
using Microsoft.AspNetCore.Mvc;
using SharedKernel.Application.Abstractions;

namespace CoreService.Application.UseCases;

public sealed class GetForumCategoriesCountRequest
{
    [FromRoute] public GuidIdList<ForumId> ForumIds { get; set; }
}

public sealed class GetForumCategoriesCountRequestValidator : AbstractValidator<GetForumCategoriesCountRequest>
{
    public GetForumCategoriesCountRequestValidator()
    {
        RuleFor(e => e.ForumIds)
            .NotEmpty();
    }
}