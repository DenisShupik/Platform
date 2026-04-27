namespace Shared.Domain.Interfaces;

public interface IHasTryFrom<T, in P> where T : IVogen<T, P>
{
    static abstract Vogen.ValueObjectOrError<T> TryFrom(P value);
}