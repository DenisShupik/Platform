using Microsoft.EntityFrameworkCore;
using CoreService.Domain.Entities;
using CoreService.Domain.ValueObjects;
using CoreService.Infrastructure.Persistence.Converters;
using SharedKernel.Domain.ValueObjects;
using Thread = CoreService.Domain.Entities.Thread;

namespace CoreService.Infrastructure.Persistence;

public sealed class ApplicationDbContext : DbContext
{
    public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
        : base(options)
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