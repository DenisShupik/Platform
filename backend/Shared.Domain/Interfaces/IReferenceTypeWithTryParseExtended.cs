using System.Diagnostics.CodeAnalysis;

namespace Shared.Domain.Interfaces;

public interface IReferenceTypeWithTryParseExtended<T> where T : class
{
    static abstract bool TryParseExtended(string? input, [NotNullWhen(true)] out T? result,
        [NotNullWhen(false)] out string? error);

    static abstract T Default { get; }
}