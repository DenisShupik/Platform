namespace SharedKernel.Application.Interfaces;

public interface IPaginationLimit
{
    static abstract int Min { get; }
    static abstract int Max { get; }
    static abstract int Default { get; }
}