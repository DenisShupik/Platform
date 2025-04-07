using CoreService.Domain.Entities;
using FluentValidation;
using Microsoft.AspNetCore.Mvc;

namespace CoreService.Application.UseCases;

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
    public long ThreadId { get; set; }

    [FromBody] public FromBody Body { get; set; }
}

public sealed class CretePostRequestValidator : AbstractValidator<CreatePostRequest>
{
    public CretePostRequestValidator()
    {
        RuleFor(e => e.ThreadId)
            .GreaterThan(0);

        RuleFor(e => e.Body.Content)
            .NotEmpty()
            .MaximumLength(Post.ContentMaxLength);
    }
}