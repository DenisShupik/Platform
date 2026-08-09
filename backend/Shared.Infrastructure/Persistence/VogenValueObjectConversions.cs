using System.Linq.Expressions;
using System.Reflection;
using LinqToDB;
using LinqToDB.Data;
using LinqToDB.DataProvider.PostgreSQL;
using LinqToDB.Mapping;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;
using Shared.Domain.Abstractions;
using Shared.Domain.Extensions;
using Shared.Domain.Interfaces;
using Shared.Infrastructure.Extensions;

namespace Shared.Infrastructure.Persistence;

public static class VogenValueObjectConversions
{
    private static readonly MethodInfo ConfigureLinqToDbMethod = typeof(VogenValueObjectConversions)
        .GetMethod(nameof(ConfigureLinqToDb), BindingFlags.NonPublic | BindingFlags.Static)!;
    private static readonly MethodInfo ConfigureIdSetLinqToDbMethod = typeof(VogenValueObjectConversions)
        .GetMethod(nameof(ConfigureIdSetLinqToDb), BindingFlags.NonPublic | BindingFlags.Static)!;
    private static readonly MethodInfo ConfigureStringLinqToDbMethod = typeof(VogenValueObjectConversions)
        .GetMethod(nameof(ConfigureStringLinqToDb), BindingFlags.NonPublic | BindingFlags.Static)!;

    public static ModelConfigurationBuilder ConfigureVogenValueObjects(
        this ModelConfigurationBuilder configurationBuilder,
        params Assembly[] assemblies)
    {
        foreach (var valueObject in Discover(assemblies))
        {
            var converterType = typeof(VogenValueConverter<,>)
                .MakeGenericType(valueObject.Type, valueObject.PrimitiveType);

            configurationBuilder.Properties(valueObject.Type).HaveConversion(converterType);
            configurationBuilder.DefaultTypeMapping(valueObject.Type).HasConversion(converterType);
        }

        return configurationBuilder;
    }

    public static MappingSchema CreateLinqToDbMappingSchema(params Assembly[] assemblies)
    {
        var mappingSchema = new MappingSchema();

        foreach (var valueObject in Discover(assemblies))
        {
            ConfigureLinqToDbMethod
                .MakeGenericMethod(valueObject.Type, valueObject.PrimitiveType)
                .Invoke(null, [mappingSchema]);

            if (valueObject.PrimitiveType == typeof(string))
            {
                ConfigureStringLinqToDbMethod
                    .MakeGenericMethod(valueObject.Type)
                    .Invoke(null, null);
            }

            if (IsId(valueObject))
            {
                ConfigureIdSetLinqToDbMethod
                    .MakeGenericMethod(valueObject.Type, valueObject.PrimitiveType)
                    .Invoke(null, null);
            }
        }

        return mappingSchema;
    }

    private static void ConfigureLinqToDb<TValueObject, TPrimitive>(MappingSchema mappingSchema)
        where TValueObject : struct, IVogen<TValueObject, TPrimitive>
    {
        Expression<Func<TValueObject, TPrimitive>> toPrimitive = value =>
            ToPrimitiveForQuery<TValueObject, TPrimitive>(value);
        Expression<Func<TPrimitive, TValueObject>> fromPrimitive = value => FromPrimitive<TValueObject, TPrimitive>(value);
        var dataType = mappingSchema.GetDataType(typeof(TPrimitive));
        var dbDataType = dataType.Type;
        var toPrimitiveConverter = toPrimitive.Compile();
        var fromPrimitiveConverter = fromPrimitive.Compile();
        Expression<Func<TValueObject, DataParameter>> toDataParameter = value =>
            new DataParameter(null, ToPrimitiveForQuery<TValueObject, TPrimitive>(value), dbDataType);
        Expression<Func<TValueObject[], DataParameter>> arrayToDataParameter = values =>
            ToArrayDataParameter<TValueObject, TPrimitive>(values);

        mappingSchema.SetScalarType(typeof(TValueObject), true);
        mappingSchema.SetScalarType(typeof(TValueObject?), true);
        mappingSchema.SetDataType(typeof(TValueObject), dataType);
        mappingSchema.SetDataType(typeof(TValueObject?), dataType);
        mappingSchema.SetConvertExpression(toPrimitive);
        mappingSchema.SetConvertExpression(fromPrimitive);
        mappingSchema.SetConvertExpression(toPrimitive, conversionType: ConversionType.ToDatabase);
        mappingSchema.SetConvertExpression(fromPrimitive, conversionType: ConversionType.FromDatabase);
        mappingSchema.SetConvertExpression(toDataParameter, conversionType: ConversionType.ToDatabase);
        mappingSchema.SetConvertExpression(arrayToDataParameter, conversionType: ConversionType.ToDatabase);
        mappingSchema.SetConverter(toPrimitiveConverter);
        mappingSchema.SetConverter(fromPrimitiveConverter);
        mappingSchema.SetConverter(toPrimitiveConverter, ConversionType.ToDatabase);
        mappingSchema.SetConverter(fromPrimitiveConverter, ConversionType.FromDatabase);
    }

    private static void ConfigureIdSetLinqToDb<TValueObject, TPrimitive>()
        where TValueObject : struct, IId, IHasTryFrom<TValueObject, TPrimitive>, IVogen<TValueObject, TPrimitive>
        where TPrimitive : ISpanParsable<TPrimitive>
    {
        Expression<Func<IdSet<TValueObject, TPrimitive>, TValueObject, bool>> contains =
            (values, value) => values.Contains(value);
        Expression<Func<IdSet<TValueObject, TPrimitive>, TValueObject, bool>> valueIsEqualToAny =
            (values, value) => Sql.Ext.PostgreSQL().ValueIsEqualToAny(
                Sql.ConvertTo<TPrimitive>.From(value),
                ToPrimitiveArray<TValueObject, TPrimitive>(values));

        LinqToDB.Linq.Expressions.MapMember(contains, valueIsEqualToAny);
    }

    private static void ConfigureStringLinqToDb<TValueObject>()
        where TValueObject : struct, IVogen<TValueObject, string>
    {
        Expression<Func<TValueObject, TValueObject, StringComparison, bool>> contains =
            (source, value, comparisonType) => source.Contains(value, comparisonType);
        Expression<Func<TValueObject, TValueObject, StringComparison, bool>> stringContains =
            (source, value, comparisonType) =>
                Sql.ConvertTo<string>.From(source)
                    .Contains(Sql.ConvertTo<string>.From(value), comparisonType);

        LinqToDB.Linq.Expressions.MapMember(contains, stringContains);
    }

    private static TValueObject FromPrimitive<TValueObject, TPrimitive>(TPrimitive value)
        where TValueObject : struct, IVogen<TValueObject, TPrimitive>
        => TValueObject.From(value);

    private static TPrimitive ToPrimitiveForQuery<TValueObject, TPrimitive>(TValueObject value)
        where TValueObject : struct, IVogen<TValueObject, TPrimitive>
        // LinqToDB evaluates default(TValueObject) while translating DefaultIfEmpty and grouping keys.
        => value.IsInitialized() ? value.Value : default!;

    private static DataParameter ToArrayDataParameter<TValueObject, TPrimitive>(
        IReadOnlyCollection<TValueObject> values)
        where TValueObject : struct, IVogen<TValueObject, TPrimitive>
        => new(null, ToPrimitiveArray<TValueObject, TPrimitive>(values))
        {
            IsArray = true
        };

    internal static TPrimitive[] ToPrimitiveArray<TValueObject, TPrimitive>(
        IReadOnlyCollection<TValueObject> values)
        where TValueObject : struct, IVogen<TValueObject, TPrimitive>
    {
        var primitives = new TPrimitive[values.Count];
        var index = 0;

        foreach (var value in values)
        {
            primitives[index++] = value.Value;
        }

        return primitives;
    }

    private static IReadOnlyCollection<ValueObjectType> Discover(IEnumerable<Assembly> assemblies)
        => assemblies
            .Distinct()
            .SelectMany(assembly => assembly.GetTypes())
            .Where(type => type is { IsValueType: true, IsGenericTypeDefinition: false })
            .Select(type => new
            {
                Type = type,
                Contract = type.GetInterfaces().SingleOrDefault(candidate =>
                    candidate.IsGenericType &&
                    candidate.GetGenericTypeDefinition() == typeof(IVogen<,>) &&
                    candidate.GetGenericArguments()[0] == type)
            })
            .Where(valueObject => valueObject.Contract is not null)
            .Select(valueObject => new ValueObjectType(
                valueObject.Type,
                valueObject.Contract!.GetGenericArguments()[1]))
            .ToArray();

    private static bool IsId(ValueObjectType valueObject)
        => typeof(IId).IsAssignableFrom(valueObject.Type) &&
           typeof(IHasTryFrom<,>)
               .MakeGenericType(valueObject.Type, valueObject.PrimitiveType)
               .IsAssignableFrom(valueObject.Type) &&
           typeof(ISpanParsable<>)
               .MakeGenericType(valueObject.PrimitiveType)
               .IsAssignableFrom(valueObject.PrimitiveType);

    private sealed record ValueObjectType(Type Type, Type PrimitiveType);
}

public sealed class VogenValueConverter<TValueObject, TPrimitive>
    : Microsoft.EntityFrameworkCore.Storage.ValueConversion.ValueConverter<TValueObject, TPrimitive>
    where TValueObject : struct, IVogen<TValueObject, TPrimitive>
{
    public VogenValueConverter() : this(null)
    {
    }

    public VogenValueConverter(ConverterMappingHints? mappingHints)
        : base(
            valueObject => valueObject.Value,
            primitive => FromPrimitive(primitive),
            mappingHints)
    {
    }

    private static TValueObject FromPrimitive(TPrimitive value)
        => TValueObject.From(value);
}
