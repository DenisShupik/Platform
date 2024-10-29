using FluentValidation;
using Microsoft.AspNetCore.Mvc;

namespace TopicService.Application.DTOs;

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

public sealed class LongIds : List<long>
{
    public static bool TryParse(string? value, IFormatProvider? provider, out LongIds? result)
    {
        result = [];
        foreach (var id in value?.Split(',') ?? [])
        {
            if (!int.TryParse(id, out var intId))
            {
                result = null;
                return false;
            }

            result.Add(intId);
        }

        return true;
    }
}