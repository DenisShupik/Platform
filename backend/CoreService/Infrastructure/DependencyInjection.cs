using System.Linq.Expressions;
using CoreService.Application.Interfaces;
using CoreService.Domain.ValueObjects;
using CoreService.Domain.Interfaces;
using CoreService.Infrastructure.Cache;
using CoreService.Infrastructure.Grpc.Contracts;
using CoreService.Infrastructure.Markdown;
using CoreService.Infrastructure.Options;
using CoreService.Infrastructure.Persistence;
using CoreService.Infrastructure.Persistence.Repositories;
using FluentValidation;
using LinqToDB.Mapping;
using Microsoft.AspNetCore.DataProtection;
using OpenTelemetry.Trace;
using ProtoBuf.Grpc.Server;
using Shared.Application.Interfaces;
using Shared.Infrastructure.Extensions;
using Shared.Infrastructure.Interfaces;
using Shared.Domain.ValueObjects;

namespace CoreService.Infrastructure;

public static class DependencyInjection
{
    public static void AddInfrastructureServices<T>(this IHostApplicationBuilder builder)
        where T : class, IDbOptions
    {
        builder.Services
            .AddValidatorsFromAssembly(typeof(DependencyInjection).Assembly, ServiceLifetime.Singleton)
            .RegisterOptions<CoreServiceOptions, CoreServiceOptionsValidator>(builder.Configuration)
            .RegisterOptions<PostContentPolicyOptions, PostContentPolicyOptionsValidator>(builder.Configuration)
            .AddSingleton<PostMarkdownProcessor>()
            .AddSingleton<IPostContentPolicy>(provider => provider.GetRequiredService<PostMarkdownProcessor>())
            .AddSingleton<IPostSearchTextProjector>(provider => provider.GetRequiredService<PostMarkdownProcessor>());

        builder.Services
            .RegisterDbContexts<ReadApplicationDbContext, WriteApplicationDbContext, T>(Constants.DatabaseSchema)
            .AddScoped<IUnitOfWork, UnitOfWork>()
            .AddScoped<IForumReadRepository, ForumReadRepository>()
            .AddScoped<IForumWriteRepository, ForumWriteRepository>()
            .AddScoped<ICategoryReadRepository, CategoryReadRepository>()
            .AddScoped<ICategoryWriteRepository, CategoryWriteRepository>()
            .AddScoped<IThreadReadRepository, ThreadReadRepository>()
            .AddScoped<IThreadWriteRepository, ThreadWriteWriteRepository>()
            .AddScoped<IPostReadRepository, PostReadRepository>()
            .AddScoped<IPostWriteRepository, PostWriteRepository>()
            .AddScoped<IPostBookmarkReadRepository, PostBookmarkReadRepository>()
            .AddScoped<IPostBookmarkWriteRepository, PostBookmarkWriteRepository>()
            .AddScoped<ISearchReadRepository, SearchReadRepository>();

        RegisterLinqToDbConverters();

        builder.Services
            .AddDataProtection()
            .SetApplicationName(nameof(CoreService));

        builder.Services
            .RegisterOpenTelemetry(builder.Environment.ApplicationName)
            .WithTracing(tracing => tracing
                .AddEntityFrameworkCoreInstrumentation()
                .AddLinqToDbInstrumentation()
            );

        builder.Services.RegisterFusionCache();
        builder.Services.RegisterCoreServiceCache(options =>
        {
            options.SetSkipMemoryCache();
            options.SetSkipDistributedCacheRead(true);
            options.SetSkipDistributedCacheWrite(false, false);
        });

        builder.Services.RegisterGrpcRuntimeTypeModel(model =>
        {
            model.MapCoreServiceTypes();
            model.CompileInPlace();
        });
        builder.Services.AddCodeFirstGrpc();
        builder.Services.AddCodeFirstGrpcReflection();
    }

    private static void RegisterLinqToDbConverters()
    {
        RegisterLinqToDbConverter<ForumTitle, string>(value => value.Value, value => ForumTitle.From(value));
        RegisterLinqToDbConverter<CategoryTitle, string>(value => value.Value, value => CategoryTitle.From(value));
        RegisterLinqToDbConverter<ThreadTitle, string>(value => value.Value, value => ThreadTitle.From(value));
        RegisterLinqToDbConverter<PostContent, string>(value => value.Value, value => PostContent.From(value));
        RegisterLinqToDbConverter<ForumId, Guid>(value => value.Value, value => ForumId.From(value));
        RegisterLinqToDbConverter<CategoryId, Guid>(value => value.Value, value => CategoryId.From(value));
        RegisterLinqToDbConverter<ThreadId, Guid>(value => value.Value, value => ThreadId.From(value));
        RegisterLinqToDbConverter<PostId, Guid>(value => value.Value, value => PostId.From(value));
        RegisterLinqToDbConverter<UserId, Guid>(value => value.Value, value => UserId.From(value));
    }

    private static void RegisterLinqToDbConverter<TValueObject, TValue>(
        Expression<Func<TValueObject, TValue>> toValue,
        Expression<Func<TValue, TValueObject>> fromValue)
        where TValueObject : struct
    {
        var dataType = MappingSchema.Default.GetDataType(typeof(TValue));
        var toValueConverter = toValue.Compile();
        var fromValueConverter = fromValue.Compile();

        MappingSchema.Default.SetScalarType(typeof(TValueObject), true);
        MappingSchema.Default.SetScalarType(typeof(TValueObject?), true);
        MappingSchema.Default.SetDataType(typeof(TValueObject), dataType);
        MappingSchema.Default.SetDataType(typeof(TValueObject?), dataType);
        MappingSchema.Default.SetConvertExpression(toValue);
        MappingSchema.Default.SetConvertExpression(fromValue);
        MappingSchema.Default.SetConvertExpression(toValue, conversionType: ConversionType.ToDatabase);
        MappingSchema.Default.SetConvertExpression(fromValue, conversionType: ConversionType.FromDatabase);
        MappingSchema.Default.SetConverter(toValueConverter);
        MappingSchema.Default.SetConverter(fromValueConverter);
        MappingSchema.Default.SetConverter(toValueConverter, ConversionType.ToDatabase);
        MappingSchema.Default.SetConverter(fromValueConverter, ConversionType.FromDatabase);
    }
}
