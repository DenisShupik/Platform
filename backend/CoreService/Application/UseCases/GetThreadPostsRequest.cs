using FluentValidation;
using Microsoft.AspNetCore.Mvc;
using SharedKernel.Paging;

namespace CoreService.Application.UseCases;

public sealed class GetThreadPostsRequest : LongKeysetPageRequest
{ 
    /// <summary>
    /// Идентификатор темы
    /// </summary>
    [FromRoute]
    public long ThreadId { get; set; }
}

public sealed class GetThreadPostsRequestValidator : LongKeysetPageRequestValidator<GetThreadPostsRequest>
{
    public GetThreadPostsRequestValidator()
    {
        RuleFor(e => e.ThreadId)
            .GreaterThan(0);
    }
}