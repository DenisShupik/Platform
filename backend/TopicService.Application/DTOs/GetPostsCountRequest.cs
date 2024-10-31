using FluentValidation;
using Microsoft.AspNetCore.Mvc;

namespace TopicService.Application.DTOs;

public sealed class GetPostsCountRequest
{
    /// <summary>
    /// Идентификатор темы
    /// </summary>
    [FromRoute]
    public long TopicId { get; set; }
}

public sealed class GetPostsCountRequestValidator : AbstractValidator<GetPostsCountRequest>
{
    public GetPostsCountRequestValidator()
    {
        RuleFor(e => e.TopicId)
            .GreaterThan(0);
    }
}