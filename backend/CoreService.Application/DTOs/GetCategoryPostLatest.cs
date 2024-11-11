using CoreService.Domain.Entities;
using FluentValidation;
using Microsoft.AspNetCore.Mvc;
using SharedKernel.Batching;

namespace CoreService.Application.DTOs;

public sealed class GetCategoryPostsRequest
{
    [FromRoute] public LongIds CategoryIds { get; set; }
    [FromQuery] public bool Latest { get; set; } = false;
}

public sealed class GetCategoryPostsRequestValidator : AbstractValidator<GetCategoryPostsRequest>
{
    public GetCategoryPostsRequestValidator()
    {
        RuleForEach(e => e.CategoryIds)
            .GreaterThan(0);
    }
}

public sealed class GetCategoryPostsResponse
{
    public long CategoryId { get; set; }
    public Post Post { get; set; }
}