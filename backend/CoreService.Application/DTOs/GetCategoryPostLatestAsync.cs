using CoreService.Domain.Entities;
using FluentValidation;
using Microsoft.AspNetCore.Mvc;
using SharedKernel.Batching;

namespace CoreService.Application.DTOs;

public sealed class GetCategoryPostLatestRequest
{
    [FromRoute] public LongIds CategoryIds { get; set; }
    [FromQuery] public bool Latest { get; set; } = false;
}

public sealed class GetCategoryPostLatestRequestValidator : AbstractValidator<GetCategoryPostLatestRequest>
{
    public GetCategoryPostLatestRequestValidator()
    {
        RuleForEach(e => e.CategoryIds)
            .GreaterThan(0);
    }
}

public sealed class GetCategoryPostLatestResponse
{
    public long CategoryId { get; set; }
    public Post Post { get; set; }
}