using FluentValidation;
using SharedKernel.Infrastructure.Interfaces;

namespace NotificationService.Infrastructure.Options;

public sealed class NotificationServiceOptions : IDbOptions
{
    public string ReadonlyConnectionString { get; set; } = null!;
    public string WritableConnectionString { get; set; } = null!;
}

public sealed class NotificationServiceOptionsValidator : AbstractValidator<NotificationServiceOptions>
{
    public NotificationServiceOptionsValidator()
    {
        RuleFor(e => e.ReadonlyConnectionString)
            .NotEmpty();

        RuleFor(e => e.WritableConnectionString)
            .NotEmpty();
    }
}