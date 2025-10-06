using CoreService.Application.Dtos;
using CoreService.Domain.Entities;
using CoreService.Domain.Enums;
using CoreService.Domain.ValueObjects;
using CoreService.Infrastructure.Persistence.Converters;
using Mapster;
using Shared.Infrastructure.Interfaces;
using Thread = CoreService.Domain.Entities.Thread;
using Microsoft.EntityFrameworkCore;
using Shared.Infrastructure.Extensions;
using UserService.Domain.ValueObjects;

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
        // configurationBuilder
        //     .Properties<CategoryPolicySetId?>()
        //     .HaveConversion<NullableCategoryPolicySetIdConverter>();
        // configurationBuilder
        //     .Properties<ThreadPolicySetId?>()
        //     .HaveConversion<NullableThreadPolicySetIdConverter>();
    }

    public DbSet<Forum> Forums => Set<Forum>();
    public DbSet<ForumCategoryAddable> ForumCategoryAddable => Set<ForumCategoryAddable>();
    public DbSet<Category> Categories => Set<Category>();
    public DbSet<CategoryThreadAddable> CategoryThreadAddable => Set<CategoryThreadAddable>();
    public DbSet<Thread> Threads => Set<Thread>();
    public DbSet<ThreadPostAddable> ThreadPostAddable => Set<ThreadPostAddable>();
    public DbSet<Post> Posts => Set<Post>();
    public DbSet<Grant> Grants => Set<Grant>();
    public DbSet<ForumRestriction> ForumRestrictions => Set<ForumRestriction>();
    public DbSet<CategoryRestriction> CategoryRestrictions => Set<CategoryRestriction>();
    public DbSet<ThreadRestriction> ThreadRestrictions => Set<ThreadRestriction>();
    public DbSet<Policy> Policies => Set<Policy>();
}

public sealed class ReadApplicationDbContext : ApplicationDbContext, IReadDbContext
{
    public ReadApplicationDbContext(DbContextOptions<ReadApplicationDbContext> options) : base(options)
    {
    }
    
     public sealed class PostThread
    {
        public Thread Thread { get; set; }
        public Post Post { get; set; }
    }

    public IQueryable<PostThread> GetAvailablePosts(UserId? userId)
    {
        var timestamp = DateTime.UtcNow;
        IQueryable<PostThread> queryable;
        if (userId == null)
        {
            queryable =
                from p in Posts
                from t in Threads.Where(e => e.ThreadId == p.ThreadId)
                from ap in Policies.Where(e => e.PolicyId == t.AccessPolicyId && e.Value == PolicyValue.Any)
                select new PostThread { Thread = t, Post = p };
        }
        else
        {
            queryable =
                from p in Posts
                from t in Threads.Where(e => e.ThreadId == p.ThreadId)
                from c in Categories.Where(e => e.CategoryId == t.CategoryId)
                from f in Forums.Where(e => e.ForumId == c.ForumId)
                from ap in Policies.Where(e => e.PolicyId == t.AccessPolicyId)
                from ag in Grants
                    .Where(e => e.UserId == userId && e.PolicyId == t.AccessPolicyId)
                    .DefaultIfEmpty()
                from fr in ForumRestrictions
                    .Where(e => e.UserId == userId && e.ForumId == f.ForumId &&
                                e.Policy == PolicyType.Access &&
                                (e.ExpiredAt == null || e.ExpiredAt > timestamp))
                    .DefaultIfEmpty()
                from cr in CategoryRestrictions
                    .Where(e => e.UserId == userId && e.CategoryId == c.CategoryId &&
                                e.Policy == PolicyType.Access &&
                                (e.ExpiredAt == null || e.ExpiredAt > timestamp))
                    .DefaultIfEmpty()
                from tr in ThreadRestrictions
                    .Where(e => e.UserId == userId && e.ThreadId == t.ThreadId &&
                                e.Policy == PolicyType.Access &&
                                (e.ExpiredAt == null || e.ExpiredAt > timestamp))
                    .DefaultIfEmpty()
                where tr == null && cr == null && fr == null &&
                      (ap.Value < PolicyValue.Granted || ag.PolicyId.SqlIsNotNull())
                select new PostThread { Thread = t, Post = p };
        }

        return queryable;
    }
}

public sealed class WriteApplicationDbContext : ApplicationDbContext, IWriteDbContext
{
    public WriteApplicationDbContext(DbContextOptions<WriteApplicationDbContext> options) : base(options)
    {
    }
}