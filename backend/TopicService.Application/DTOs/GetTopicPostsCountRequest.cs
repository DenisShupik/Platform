using FluentValidation;
using Microsoft.AspNetCore.Mvc;
using SharedKernel.Batching;

namespace TopicService.Application.DTOs;

public sealed class GetTopicPostsCountRequest
{
    /// <summary>
    /// Идентификатор темы
    /// </summary>
    [FromRoute]
    public LongIds TopicIds { get; set; }
}

public sealed class GetTopicPostsCountRequestValidator : AbstractValidator<GetTopicPostsCountRequest>
{
    public GetTopicPostsCountRequestValidator()
    {
        RuleForEach(e => e.TopicIds)
            .GreaterThan(0);
    }
}