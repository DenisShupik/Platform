namespace Shared.Application.Interfaces;

public interface IPaginationLimit
{
    static abstract int Min { get; }
    static abstract int Max { get; }
}