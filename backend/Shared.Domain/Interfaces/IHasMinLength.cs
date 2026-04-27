namespace Shared.Domain.Interfaces;

public interface IHasMinLength
{
    static abstract int MinLength { get; }
}