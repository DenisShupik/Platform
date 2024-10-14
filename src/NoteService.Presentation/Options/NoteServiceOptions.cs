using Common.Interfaces;
using FluentValidation;

namespace NoteService.Presentation.Options;

public sealed class NoteServiceOptions : IDbOptions
{
    public string ConnectionString { get; set; } = null!;
}

public sealed class NoteServiceOptionsValidator : AbstractValidator<NoteServiceOptions>
{
    public NoteServiceOptionsValidator()
    {
        RuleFor(e => e.ConnectionString)
            .NotEmpty();
    }
}