using SharedKernel;
using FluentValidation;
using Microsoft.AspNetCore.Mvc;
using SharedKernel.Paging;

namespace CoreService.Application.DTOs;

public sealed class GetCategoryThreadsRequest : LongKeysetPageRequest
{
    /// <summary>
    /// Идентификатор категории
    /// </summary>
    [FromRoute]
    public long CategoryId { get; set; }
}

public sealed class GetCategoryThreadsRequestValidator : LongKeysetPageRequestValidator<GetCategoryThreadsRequest>
{
    public GetCategoryThreadsRequestValidator()
    {
        RuleFor(e => e.CategoryId)
            .GreaterThan(0);
    }
}