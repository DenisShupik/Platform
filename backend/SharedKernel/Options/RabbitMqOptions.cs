using FluentValidation;

namespace SharedKernel.Options;

public sealed class RabbitMqOptions
{
    public string Host { get; set; }
    public ushort Port { get; set; } = 5672;
    public string VirtualHost { get; set; } = "/";
    public string Username { get; set; }
    public string Password { get; set; }
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