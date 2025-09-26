using FluentValidation;

namespace Shared.Infrastructure.Options;

public sealed class RabbitMqOptions
{
    public required string Host { get; init; }
    public ushort Port { get; init; } = 5672;
    public string VirtualHost { get; init; } = "/";
    public required string Username { get; init; }
    public required string Password { get; init; }
}

public sealed class RabbitMqOptionsValidator : AbstractValidator<RabbitMqOptions>
{
    public RabbitMqOptionsValidator()
    {
        RuleFor(options => options.Host)
            .NotEmpty();

        RuleFor(options => options.VirtualHost)
            .NotEmpty();

        RuleFor(options => options.Username)
            .NotEmpty();

        RuleFor(options => options.Password)
            .NotEmpty();
    }
}