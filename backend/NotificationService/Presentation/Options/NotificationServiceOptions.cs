using FluentValidation;
using SharedKernel.Infrastructure.Interfaces;

namespace NotificationService.Presentation.Options;

public sealed class NotificationServiceOptions : IDbOptions
{
    public string ConnectionString { get; set; } = null!;
}

public sealed class NotificationServiceOptionsValidator : AbstractValidator<NotificationServiceOptions>
{
    public NotificationServiceOptionsValidator()
    {
        RuleFor(e => e.ConnectionString)
            .NotEmpty();
    }
}