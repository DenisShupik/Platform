using FluentValidation;

namespace Common.Options;

public sealed class RedisOptions
{
    public string ConnectionString { get; set; }
}

public sealed class RedisOptionsValidator : AbstractValidator<RedisOptions>
{
    public RedisOptionsValidator()
    {
        RuleFor(options => options.ConnectionString)
            .NotEmpty();
    }
}