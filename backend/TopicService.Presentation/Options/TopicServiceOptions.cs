using FluentValidation;
using SharedKernel.Interfaces;

namespace TopicService.Presentation.Options;

public sealed class TopicServiceOptions : IDbOptions
{
    public string ConnectionString { get; set; } = null!;
}

public sealed class TopicServiceOptionsValidator : AbstractValidator<TopicServiceOptions>
{
    public TopicServiceOptionsValidator()
    {
        RuleFor(e => e.ConnectionString)
            .NotEmpty();
    }
}