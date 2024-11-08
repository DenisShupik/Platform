using FluentValidation;
using Microsoft.AspNetCore.Mvc;
using SharedKernel.Batching;

namespace CoreService.Application.DTOs;

public sealed class GetCategoriesStatsRequest
{
    [FromRoute] public LongIds CategoryIds { get; set; }
}

public sealed class GetCategoryStatsRequestValidator : AbstractValidator<GetCategoriesStatsRequest>
{
    public GetCategoryStatsRequestValidator()
    {
        RuleForEach(e => e.CategoryIds)
            .GreaterThan(0);
    }
}