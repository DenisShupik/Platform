using CoreService.Application.Dtos;
using CoreService.Domain.Entities;
using CoreService.Infrastructure.Persistence.Converters;
using Mapster;
using Shared.Infrastructure.Interfaces;
using Thread = CoreService.Domain.Entities.Thread;
using Microsoft.EntityFrameworkCore;

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
        
        // TODO: Mapster иначе не может построить проекцию
        TypeAdapterConfig.GlobalSettings
            .ForType<PostAddedActivity, ActivityDto>()
            .MapWith(src => new PostAddedActivityDto
            {
                ForumId = src.ForumId,
                CategoryId = src.CategoryId,
                ThreadId = src.ThreadId,
                PostId = src.PostId,
                OccurredBy = src.OccurredBy,
                OccurredAt = src.OccurredAt
            });
        
        TypeAdapterConfig.GlobalSettings.CompileProjection();
    }
    
    protected override void ConfigureConventions(ModelConfigurationBuilder configurationBuilder)
    {
        base.ConfigureConventions(configurationBuilder);
        configurationBuilder.RegisterAllInVogenEfCoreConverters();
    }

    public DbSet<Forum> Forums => Set<Forum>();
    public DbSet<ForumCategoryAddable> ForumCategoryAddable => Set<ForumCategoryAddable>();
    public DbSet<Category> Categories => Set<Category>();
    public DbSet<CategoryThreadAddable> CategoryThreadAddable => Set<CategoryThreadAddable>();
    public DbSet<Thread> Threads => Set<Thread>();
    public DbSet<ThreadPostAddable> ThreadPostAddable => Set<ThreadPostAddable>();
    public DbSet<Post> Posts => Set<Post>();
}

public sealed class ReadApplicationDbContext : ApplicationDbContext, IReadDbContext
{
    public ReadApplicationDbContext(DbContextOptions<ReadApplicationDbContext> options) : base(options)
    {
    }
}

public sealed class WriteApplicationDbContext : ApplicationDbContext, IWriteDbContext
{
    public WriteApplicationDbContext(DbContextOptions<WriteApplicationDbContext> options) : base(options)
    {
    }
}