using CoreService.Domain.ValueObjects;
using FluentValidation;
using Microsoft.AspNetCore.Mvc;
using SharedKernel.Application.Abstractions;

namespace CoreService.Application.UseCases;

public sealed class GetThreadPostsLatestRequest
{
    /// <summary>
    /// Идентификатор темы
    /// </summary>
    [FromRoute]
    public IdList<ThreadId> ThreadIds { get; set; }
}

public sealed class GetThreadPostsLatestRequestValidator : AbstractValidator<GetThreadPostsLatestRequest>
{
    public GetThreadPostsLatestRequestValidator()
    {
        RuleFor(e => e.ThreadIds)
            .NotEmpty();
    }
}