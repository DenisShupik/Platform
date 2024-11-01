using FluentValidation;
using Microsoft.AspNetCore.Mvc;
using TopicService.Domain.Entities;

namespace TopicService.Application.DTOs;

public sealed class CreatePostRequest
{
    public sealed class FromBody
    {
        /// <summary>
        /// Содержимое сообщения
        /// </summary>
        public string Content { get; set; }
    }

    /// <summary>
    /// Идентификатор темы
    /// </summary>
    [FromRoute]
    public long TopicId { get; set; }

    [FromBody] public FromBody Body { get; set; }
}

public sealed class CretePostRequestValidator : AbstractValidator<CreatePostRequest>
{
    public CretePostRequestValidator()
    {
        RuleFor(e => e.TopicId)
            .GreaterThan(0);

        RuleFor(e => e.Body.Content)
            .NotEmpty()
            .MaximumLength(Post.ContentMaxLength);
    }
}