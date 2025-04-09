using CoreService.Application.Dtos;
using CoreService.Domain.ValueObjects;
using FluentValidation;
using Microsoft.AspNetCore.Mvc;
using SharedKernel.Application.Abstractions;

namespace CoreService.Application.UseCases;

public sealed class GetCategoryPostsRequest
{
    [FromRoute] public GuidIdList<CategoryId> CategoryIds { get; set; }
    [FromQuery] public bool Latest { get; set; } = false;
}

public sealed class GetCategoryPostsRequestValidator : AbstractValidator<GetCategoryPostsRequest>
{
    public GetCategoryPostsRequestValidator()
    {
        RuleFor(e => e.CategoryIds)
            .NotEmpty();
    }
}

public sealed class GetCategoryPostsResponse
{
    public CategoryId CategoryId { get; set; }
    public PostDto Post { get; set; }
}