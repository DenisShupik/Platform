using FluentValidation;
using TopicService.Domain.Entities;

namespace TopicService.Application.DTOs;

public sealed class CreatePostRequest
{
    /// <summary>
    /// Идентификатор темы
    /// </summary>
    public long TopicId { get; set; }

    /// <summary>
    /// Содержимое сообщения
    /// </summary>
    public string Content { get; set; }
}

public sealed class CretePostRequestValidator : AbstractValidator<CreatePostRequest>
{
    public CretePostRequestValidator()
    {
        RuleFor(e => e.TopicId)
            .GreaterThan(0);

        RuleFor(e => e.Content)
            .NotEmpty()
            .MaximumLength(Post.ContentMaxLength);
    }
}