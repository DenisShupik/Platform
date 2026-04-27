using System.Diagnostics.CodeAnalysis;

namespace Shared.Domain.Interfaces;

public interface IValueTypeWithTryParseExtended<T> where T : struct
{
    static abstract bool TryParseExtended(string? input, [NotNullWhen(true)] out T? result,
        [NotNullWhen(false)] out string? error);
}