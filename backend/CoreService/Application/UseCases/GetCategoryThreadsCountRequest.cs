using FluentValidation;
using Microsoft.AspNetCore.Mvc;
using SharedKernel.Batching;

namespace CoreService.Application.UseCases;

public sealed class GetCategoryThreadsCountRequest
{
    /// <summary>
    /// Идентификаторы категории
    /// </summary>
    [FromRoute]
    public LongIds CategoryIds { get; set; }
}

public sealed class GetCategoryThreadsCountRequestValidator : AbstractValidator<GetCategoryThreadsCountRequest>
{
    public GetCategoryThreadsCountRequestValidator()
    {
        RuleForEach(e => e.CategoryIds)
            .GreaterThan(0);
    }
}