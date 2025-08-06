using Microsoft.EntityFrameworkCore;
using CoreService.Domain.Entities;
using CoreService.Domain.ValueObjects;
using CoreService.Infrastructure.Persistence.Converters;
using Mapster;
using SharedKernel.Infrastructure.Interfaces;
using UserService.Domain.ValueObjects;
using Thread = CoreService.Domain.Entities.Thread;

namespace CoreService.Infrastructure.Persistence;

public abstract class ApplicationDbContext : DbContext
{
    protected ApplicationDbContext(DbContextOptions<ApplicationDbContext> options) : base(options)
    {
    }

    protected ApplicationDbContext(DbContextOptions options) : base(options)
    {
    }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);
        modelBuilder.HasDefaultSchema(Constants.DatabaseSchema);
        modelBuilder.ApplyConfigurationsFromAssembly(typeof(ApplicationDbContext).Assembly);

        modelBuilder.Entity<ForumCategoryAddable>(builder =>
        {
            var entityTypeBuilder = modelBuilder.Entity<Forum>();
            var medMetadata = entityTypeBuilder.Metadata;

            builder.ToTable(medMetadata.GetTableName());

            builder.HasKey(e => e.ForumId);

            builder
                .Property(e => e.ForumId)
                .ValueGeneratedNever();
        });

        modelBuilder.Entity<CategoryThreadAddable>(builder =>
        {
            var entityTypeBuilder = modelBuilder.Entity<Category>();
            var medMetadata = entityTypeBuilder.Metadata;

            builder.ToTable(medMetadata.GetTableName());

            builder.HasKey(e => e.CategoryId);

            builder
                .Property(e => e.CategoryId)
                .ValueGeneratedNever();
        });

        modelBuilder.Entity<ThreadPostAddable>(builder =>
        {
            var entityTypeBuilder = modelBuilder.Entity<Thread>();
            var medMetadata = entityTypeBuilder.Metadata;


            builder.ToTable(medMetadata.GetTableName());

            builder.HasKey(e => e.ThreadId);

            builder
                .Property(e => e.ThreadId)
                .ValueGeneratedNever();

            var nextPostId = entityTypeBuilder.Property(e => e.NextPostId).Metadata.GetColumnName();
            builder.Property(e => e.NextPostId).HasColumnName(nextPostId);
            entityTypeBuilder.Property(e => e.NextPostId).HasColumnName(nextPostId);

            var status = entityTypeBuilder.Property(e => e.Status).Metadata.GetColumnName();
            builder.Property(e => e.Status).HasColumnName(status);
            entityTypeBuilder.Property(e => e.Status).HasColumnName(status);

            var createdBy = entityTypeBuilder.Property(e => e.CreatedBy).Metadata.GetColumnName();
            builder.Property(e => e.CreatedBy).HasColumnName(createdBy);
            entityTypeBuilder.Property(e => e.CreatedBy).HasColumnName(createdBy);
        });

        TypeAdapterConfig.GlobalSettings
            .ForType<Forum, Forum>()
            .MapWith(src => src);

        TypeAdapterConfig.GlobalSettings
            .ForType<Thread, Thread>()
            .MapWith(src => src);
    }

    protected override void ConfigureConventions(ModelConfigurationBuilder configurationBuilder)
    {
        base.ConfigureConventions(configurationBuilder);
        configurationBuilder.RegisterAllInVogenEfCoreConverters();

        configurationBuilder
            .Properties<ForumId>()
            .HaveConversion<NullableForumIdConverter>();

        configurationBuilder
            .Properties<UserId>()
            .HaveConversion<NullableUserIdConverter>();
    }

    public DbSet<Forum> Forums => Set<Forum>();
    public DbSet<ForumCategoryAddable> ForumCategoryAddable => Set<ForumCategoryAddable>();
    public DbSet<Category> Categories => Set<Category>();
    public DbSet<CategoryThreadAddable> CategoryThreadAddable => Set<CategoryThreadAddable>();
    public DbSet<Thread> Threads => Set<Thread>();
    public DbSet<ThreadPostAddable> ThreadPostAddable => Set<ThreadPostAddable>();
    public DbSet<Post> Posts => Set<Post>();
}

public sealed class ReadonlyApplicationDbContext : ApplicationDbContext, IReadonlyDbContext
{
    public ReadonlyApplicationDbContext(DbContextOptions<ReadonlyApplicationDbContext> options) : base(options)
    {
    }
}

public sealed class WritableApplicationDbContext : ApplicationDbContext, IWritableDbContext
{
    public WritableApplicationDbContext(DbContextOptions<WritableApplicationDbContext> options) : base(options)
    {
    }
}