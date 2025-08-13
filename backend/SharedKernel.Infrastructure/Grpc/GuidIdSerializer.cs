using System.Buffers.Binary;
using ProtoBuf;
using ProtoBuf.Meta;
using ProtoBuf.Serializers;

namespace SharedKernel.Infrastructure.Grpc;

public sealed class GuidIdSerializer<T> : ISerializer<T> where T : IVogen<T, Guid>
{
    private const int FieldLo = 1;
    private const int FieldHi = 2;

    SerializerFeatures ISerializer<T>.Features
        => SerializerFeatures.CategoryMessage | SerializerFeatures.WireTypeString;

    public static void Configure(RuntimeTypeModel model)
    {
        var type = model.Add<T>(false);
        type.SerializerType = typeof(GuidIdSerializer<T>);

        var nullableType = model.Add<T?>(applyDefaultBehaviour: false);
        nullableType.SerializerType = typeof(GuidIdSerializer<T>);
    }

    T ISerializer<T>.Read(ref ProtoReader.State state, T value)
    {
        ulong lo = 0;
        ulong hi = 0;
        int fieldNumber;
        while ((fieldNumber = state.ReadFieldHeader()) > 0)
        {
            switch (fieldNumber)
            {
                case FieldLo:
                    lo = state.ReadUInt64();
                    break;
                case FieldHi:
                    hi = state.ReadUInt64();
                    break;
                default:
                    state.SkipField();
                    break;
            }
        }

        Span<byte> buffer = stackalloc byte[16];
        BinaryPrimitives.WriteUInt64LittleEndian(buffer, lo);
        BinaryPrimitives.WriteUInt64LittleEndian(buffer[8..], hi);

        return T.From(new Guid(buffer));
    }

    void ISerializer<T>.Write(ref ProtoWriter.State state, T value)
    {
        var guid = value.Value;
        Span<byte> buffer = stackalloc byte[16];
        guid.TryWriteBytes(buffer);

        var lo = BinaryPrimitives.ReadUInt64LittleEndian(buffer);
        var hi = BinaryPrimitives.ReadUInt64LittleEndian(buffer[8..]);

        state.WriteFieldHeader(FieldLo, WireType.Fixed64);
        state.WriteUInt64(lo);

        state.WriteFieldHeader(FieldHi, WireType.Fixed64);
        state.WriteUInt64(hi);
    }
}