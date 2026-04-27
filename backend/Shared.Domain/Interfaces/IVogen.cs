// Должен находиться в глобальном пространстве
public interface IVogen<TSelf, TPrimitive>
    where TSelf : IVogen<TSelf, TPrimitive>
{
    static abstract explicit operator TSelf(TPrimitive value);
    static abstract explicit operator TPrimitive(TSelf value);
    static abstract bool operator ==(TSelf left, TSelf right);
    static abstract bool operator !=(TSelf left, TSelf right);
    static abstract bool operator ==(TSelf left, TPrimitive right);
    static abstract bool operator !=(TSelf left, TPrimitive right);
    static abstract bool operator ==(TPrimitive left, TSelf right);
    static abstract bool operator !=(TPrimitive left, TSelf right);
    static abstract TSelf From(TPrimitive value);
    static abstract bool TryFrom(TPrimitive value, out TSelf? vo);
    TPrimitive Value { get; }

    bool IsInitialized();
}