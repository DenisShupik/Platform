using FluentValidation;
using Microsoft.AspNetCore.Mvc;
using SharedKernel.Paging;

namespace CoreService.Application.UseCases;

public sealed class GetForumCategoriesRequest : LongKeysetPageRequest
{
    [FromRoute] public long ForumId { get; set; }
}

public sealed class GetForumCategoriesRequestValidator : LongKeysetPageRequestValidator<GetForumCategoriesRequest>
{
    public GetForumCategoriesRequestValidator()
    {
        RuleFor(e => e.ForumId)
            .GreaterThan(0);
    }
}