using CoreService.Domain.ValueObjects;
using FluentValidation;
using Microsoft.AspNetCore.Mvc;
using SharedKernel.Application.Abstractions;

namespace CoreService.Application.UseCases;

public sealed class GetThreadPostsCountRequest
{
    /// <summary>
    /// Идентификатор темы
    /// </summary>
    [FromRoute]
    public GuidIdList<ThreadId> ThreadIds { get; set; }
}

public sealed class GetThreadPostsCountRequestValidator : AbstractValidator<GetThreadPostsCountRequest>
{
    public GetThreadPostsCountRequestValidator()
    {
        RuleFor(e => e.ThreadIds)
            .NotEmpty();
    }
}