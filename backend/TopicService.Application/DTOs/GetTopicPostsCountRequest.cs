using FluentValidation;
using Microsoft.AspNetCore.Mvc;

namespace TopicService.Application.DTOs;

public sealed class GetTopicPostsCountRequest
{
    /// <summary>
    /// Идентификатор темы
    /// </summary>
    [FromRoute]
    public long TopicId { get; set; }
}

public sealed class GetTopicPostsCountRequestValidator : AbstractValidator<GetTopicPostsCountRequest>
{
    public GetTopicPostsCountRequestValidator()
    {
        RuleFor(e => e.TopicId)
            .GreaterThan(0);
    }
}