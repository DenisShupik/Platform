using FluentValidation;
using Microsoft.AspNetCore.Mvc;
using SharedKernel.Batching;
using SharedKernel.Paging;

namespace UserService.Application.UseCases;

public sealed class GetUsersRequest : GuidKeysetPageRequest
{ 
    /// <summary>
    /// Идентификатор темы
    /// </summary>
    [FromQuery]
    public GuidIds Ids { get; set; }
}

public sealed class GetUsersRequestValidator : GuidKeysetPageRequestValidator<GetUsersRequest>
{
    public GetUsersRequestValidator()
    {
        RuleForEach(e => e.Ids)
            .NotEmpty();
    }
}