using System.Text.RegularExpressions;

namespace Shared.Domain.Interfaces;

public interface IHasRegex
{
    static abstract Regex Regex { get; }
    static abstract string RegexValidationError { get; }
}