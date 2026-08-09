using System.Linq.Expressions;
using System.Reflection;
using LinqToDB.Data;
using LinqToDB.Mapping;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.ChangeTracking;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;
using Shared.Domain.Abstractions;

namespace Shared.Infrastructure.Persistence;

public static class EnumSetConversions
{
    private static readonly MethodInfo ConfigureDiscoveredLinqToDbMethod = typeof(EnumSetConversions)
        .GetMethod(nameof(ConfigureDiscoveredLinqToDb), BindingFlags.NonPublic | BindingFlags.Static)!;

    public static ModelConfigurationBuilder ConfigureEnumSets(
        this ModelConfigurationBuilder configurationBuilder,
        params Assembly[] assemblies)
    {
        foreach (var enumType in Discover(assemblies))
        {
            var databaseType = GetPostgreSqlStorageType(enumType);
            var enumSetType = typeof(EnumSet<>).MakeGenericType(enumType);
            var converterType = typeof(EnumSetValueConverter<,>).MakeGenericType(enumType, databaseType);
            var comparerType = typeof(EnumSetValueComparer<>).MakeGenericType(enumType);

            configurationBuilder
                .Properties(enumSetType)
                .HaveConversion(converterType, comparerType);
        }

        return configurationBuilder;
    }

    public static MappingSchema ConfigureEnumSets(
        this MappingSchema mappingSchema,
        params Assembly[] assemblies)
    {
        foreach (var enumType in Discover(assemblies))
        {
            ConfigureDiscoveredLinqToDbMethod
                .MakeGenericMethod(enumType, GetPostgreSqlStorageType(enumType))
                .Invoke(null, [mappingSchema]);
        }

        return mappingSchema;
    }

    public static void ConfigureEnumSet<TEnum, TDatabase>(
        this MappingSchema mappingSchema,
        Expression<Func<TEnum, TDatabase>> toDatabase,
        Expression<Func<TDatabase, TEnum>> fromDatabase)
        where TEnum : struct, Enum
    {
        var toDatabaseConverter = toDatabase.Compile();
        var fromDatabaseConverter = fromDatabase.Compile();

        Expression<Func<EnumSet<TEnum>, TDatabase[]>> toDatabaseArray = values =>
            ToDatabaseArray(values, toDatabaseConverter);
        Expression<Func<TDatabase[], EnumSet<TEnum>>> fromDatabaseArray = values =>
            FromDatabaseArray(values, fromDatabaseConverter);
        Expression<Func<EnumSet<TEnum>, DataParameter>> toDataParameter = values =>
            ToDataParameter(values, toDatabaseConverter);
        var toDatabaseArrayConverter = toDatabaseArray.Compile();
        var fromDatabaseArrayConverter = fromDatabaseArray.Compile();
        var databaseType = mappingSchema.GetDataType(typeof(TDatabase));

        mappingSchema.SetScalarType(typeof(TEnum), true);
        mappingSchema.SetDataType(typeof(TEnum), databaseType);
        mappingSchema.SetScalarType(typeof(EnumSet<TEnum>), true);
        mappingSchema.SetConvertExpression(toDatabase);
        mappingSchema.SetConvertExpression(fromDatabase);
        mappingSchema.SetConvertExpression(toDatabase, conversionType: ConversionType.ToDatabase);
        mappingSchema.SetConvertExpression(fromDatabase, conversionType: ConversionType.FromDatabase);
        mappingSchema.SetConvertExpression(toDatabaseArray, conversionType: ConversionType.ToDatabase);
        mappingSchema.SetConvertExpression(fromDatabaseArray, conversionType: ConversionType.FromDatabase);
        mappingSchema.SetConvertExpression(toDataParameter, conversionType: ConversionType.ToDatabase);
        mappingSchema.SetConverter(toDatabaseConverter);
        mappingSchema.SetConverter(fromDatabaseConverter);
        mappingSchema.SetConverter(toDatabaseConverter, ConversionType.ToDatabase);
        mappingSchema.SetConverter(fromDatabaseConverter, ConversionType.FromDatabase);
        mappingSchema.SetConverter(toDatabaseArrayConverter, ConversionType.ToDatabase);
        mappingSchema.SetConverter(fromDatabaseArrayConverter, ConversionType.FromDatabase);
    }

    private static void ConfigureDiscoveredLinqToDb<TEnum, TDatabase>(MappingSchema mappingSchema)
        where TEnum : struct, Enum
        where TDatabase : struct
    {
        var enumValue = Expression.Parameter(typeof(TEnum), "value");
        var databaseValue = Expression.Parameter(typeof(TDatabase), "value");
        var toDatabase = Expression.Lambda<Func<TEnum, TDatabase>>(
            Expression.Convert(enumValue, typeof(TDatabase)),
            enumValue);
        var fromDatabase = Expression.Lambda<Func<TDatabase, TEnum>>(
            Expression.Convert(databaseValue, typeof(TEnum)),
            databaseValue);

        mappingSchema.ConfigureEnumSet(toDatabase, fromDatabase);
    }

    private static IReadOnlyCollection<Type> Discover(IEnumerable<Assembly> assemblies)
        => assemblies
            .Distinct()
            .SelectMany(assembly => assembly.GetTypes())
            .SelectMany(type => type.GetProperties(
                BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic))
            .Select(property => property.PropertyType)
            .Where(type => type.IsGenericType && type.GetGenericTypeDefinition() == typeof(EnumSet<>))
            .Select(type => type.GetGenericArguments()[0])
            .Where(type => type.IsEnum)
            .Distinct()
            .ToArray();

    private static Type GetPostgreSqlStorageType(Type enumType)
        => Type.GetTypeCode(Enum.GetUnderlyingType(enumType)) switch
        {
            TypeCode.SByte or TypeCode.Byte or TypeCode.Int16 => typeof(short),
            TypeCode.UInt16 or TypeCode.Int32 => typeof(int),
            TypeCode.UInt32 or TypeCode.Int64 => typeof(long),
            TypeCode.UInt64 => typeof(decimal),
            _ => throw new NotSupportedException($"Unsupported underlying type for {enumType.FullName}")
        };

    private static DataParameter ToDataParameter<TSource, TDatabase>(
        IReadOnlyCollection<TSource> values,
        Func<TSource, TDatabase> converter)
        => new(null, ToDatabaseArray(values, converter))
        {
            IsArray = true
        };

    private static TDatabase[] ToDatabaseArray<TSource, TDatabase>(
        IReadOnlyCollection<TSource> values,
        Func<TSource, TDatabase> converter)
        => ConvertArray(values, converter);

    internal static TTarget[] ConvertArray<TSource, TTarget>(
        IReadOnlyCollection<TSource> values,
        Func<TSource, TTarget> converter)
    {
        var result = new TTarget[values.Count];
        var index = 0;

        foreach (var value in values)
        {
            result[index++] = converter(value);
        }

        return result;
    }

    private static EnumSet<TEnum> FromDatabaseArray<TEnum, TDatabase>(
        IReadOnlyCollection<TDatabase> values,
        Func<TDatabase, TEnum> converter)
        where TEnum : struct, Enum
    {
        var result = new HashSet<TEnum>(values.Count);

        foreach (var value in values)
        {
            result.Add(converter(value));
        }

        return new EnumSet<TEnum>(result);
    }
}

public sealed class EnumSetValueConverter<TEnum, TDatabase>
    : Microsoft.EntityFrameworkCore.Storage.ValueConversion.ValueConverter<EnumSet<TEnum>, TDatabase[]>
    where TEnum : struct, Enum
    where TDatabase : struct
{
    private static readonly Func<TEnum, TDatabase> ToDatabaseValue = CreateConverter<TEnum, TDatabase>();
    private static readonly Func<TDatabase, TEnum> FromDatabaseValue = CreateConverter<TDatabase, TEnum>();

    public EnumSetValueConverter() : this(null)
    {
    }

    public EnumSetValueConverter(ConverterMappingHints? mappingHints)
        : base(
            values => ToDatabase(values),
            values => FromDatabase(values),
            mappingHints)
    {
    }

    private static TDatabase[] ToDatabase(EnumSet<TEnum> values)
        => EnumSetConversions.ConvertArray(values, ToDatabaseValue);

    private static EnumSet<TEnum> FromDatabase(TDatabase[] values)
    {
        var result = new HashSet<TEnum>(values.Length);

        foreach (var value in values)
        {
            result.Add(FromDatabaseValue(value));
        }

        return new EnumSet<TEnum>(result);
    }

    private static Func<TSource, TTarget> CreateConverter<TSource, TTarget>()
    {
        var value = Expression.Parameter(typeof(TSource), "value");
        return Expression
            .Lambda<Func<TSource, TTarget>>(Expression.Convert(value, typeof(TTarget)), value)
            .Compile();
    }
}

public sealed class EnumSetValueComparer<TEnum> : ValueComparer<EnumSet<TEnum>>
    where TEnum : struct, Enum
{
    public EnumSetValueComparer()
        : base(
            (left, right) => ReferenceEquals(left, right)
                             || (left != null && right != null && left.SetEquals(right)),
            values => values.Count == 0
                ? 0
                : values
                    .OrderBy(value => Convert.ToInt64(value))
                    .Aggregate(0, (hash, value) => HashCode.Combine(hash, value.GetHashCode())),
            values => new(values))
    {
    }
}
