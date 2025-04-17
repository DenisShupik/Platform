using Microsoft.EntityFrameworkCore;
using CoreService.Domain.Entities;
using CoreService.Domain.ValueObjects;
using CoreService.Infrastructure.Persistence.Converters;
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

        modelBuilder.Entity<ThreadPostAddable>(builder =>
        {
            var entityTypeBuilder = modelBuilder.Entity<Thread>();
            var medMetadata = entityTypeBuilder.Metadata;
            var postIdProp = entityTypeBuilder.Property(e => e.PostIdSeq).Metadata.GetColumnName();

            builder.ToTable(medMetadata.GetTableName());

            builder.HasKey(e => e.ThreadId);

            builder
                .Property(e => e.ThreadId)
                .ValueGeneratedNever();

            builder.Property(e => e.PostIdSeq).HasColumnName(postIdProp);
            entityTypeBuilder.Property(e => e.PostIdSeq).HasColumnName(postIdProp);
        });
    }

    protected override void ConfigureConventions(ModelConfigurationBuilder configurationBuilder)
    {
        base.ConfigureConventions(configurationBuilder);
        configurationBuilder.RegisterAllInVogenEfCoreConverters();

        configurationBuilder
            .Properties<ForumId>()
            .HaveConversion<NullableForumIdConverter>();
    }

    public DbSet<Forum> Forums => Set<Forum>();
    public DbSet<Category> Categories => Set<Category>();
    public DbSet<Thread> Threads => Set<Thread>();
    public DbSet<ThreadPostAddable> ThreadPostAddable => Set<ThreadPostAddable>();
    public DbSet<Post> Posts => Set<Post>();
}