using CoreService.Application.Dtos;
using CoreService.Domain.Entities;
using CoreService.Domain.Enums;
using CoreService.Domain.ValueObjects;
using CoreService.Infrastructure.Persistence.Abstractions;
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

            var readPolicyId = entityTypeBuilder.Property(e => e.ReadPolicyId).Metadata.GetColumnName();
            builder.Property(e => e.ReadPolicyId).HasColumnName(readPolicyId);
            entityTypeBuilder.Property(e => e.ReadPolicyId).HasColumnName(readPolicyId);

            var threadCreatePolicyId = entityTypeBuilder.Property(e => e.ThreadCreatePolicyId).Metadata.GetColumnName();
            builder.Property(e => e.ThreadCreatePolicyId).HasColumnName(threadCreatePolicyId);
            entityTypeBuilder.Property(e => e.ThreadCreatePolicyId).HasColumnName(threadCreatePolicyId);

            var postCreatePolicyId = entityTypeBuilder.Property(e => e.PostCreatePolicyId).Metadata.GetColumnName();
            builder.Property(e => e.PostCreatePolicyId).HasColumnName(postCreatePolicyId);
            entityTypeBuilder.Property(e => e.PostCreatePolicyId).HasColumnName(postCreatePolicyId);
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

            var readPolicyId = entityTypeBuilder.Property(e => e.ReadPolicyId).Metadata.GetColumnName();
            builder.Property(e => e.ReadPolicyId).HasColumnName(readPolicyId);
            entityTypeBuilder.Property(e => e.ReadPolicyId).HasColumnName(readPolicyId);

            var postCreatePolicyId = entityTypeBuilder.Property(e => e.PostCreatePolicyId).Metadata.GetColumnName();
            builder.Property(e => e.PostCreatePolicyId).HasColumnName(postCreatePolicyId);
            entityTypeBuilder.Property(e => e.PostCreatePolicyId).HasColumnName(postCreatePolicyId);
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

        TypeAdapterConfig.GlobalSettings.CompileProjection();
    }

    protected override void ConfigureConventions(ModelConfigurationBuilder configurationBuilder)
    {
        base.ConfigureConventions(configurationBuilder);
        configurationBuilder.RegisterAllInVogenEfCoreConverters();
    }

    public DbSet<Portal> Portal => Set<Portal>();
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

    public IQueryable<ProjectionWithAccessInfo<Forum>> GetForumsWithAccessInfo(UserId? userId)
    {
        var timestamp = DateTime.UtcNow;
        IQueryable<ProjectionWithAccessInfo<Forum>> queryable;
        if (userId == null)
        {
            queryable =
                from f in Forums
                from rp in Policies.Where(e => e.PolicyId == f.ReadPolicyId)
                select new ProjectionWithAccessInfo<Forum>
                {
                    Projection = f,
                    ReadPolicyId = rp.PolicyId,
                    ReadPolicyValue = rp.Value,
                    HasGrant = false,
                    HasRestriction = false
                };
        }
        else
        {
            queryable =
                from f in Forums
                from ap in Policies.Where(e => e.PolicyId == f.ReadPolicyId)
                from ag in Grants
                    .Where(e => e.UserId == userId && e.PolicyId == f.ReadPolicyId)
                    .DefaultIfEmpty()
                from fr in ForumRestrictions
                    .Where(e => e.UserId == userId && e.ForumId == f.ForumId &&
                                e.Policy == PolicyType.Read &&
                                (e.ExpiredAt == null || e.ExpiredAt > timestamp))
                    .DefaultIfEmpty()
                select new ProjectionWithAccessInfo<Forum>
                {
                    Projection = f,
                    ReadPolicyId = ap.PolicyId,
                    ReadPolicyValue = ap.Value,
                    HasGrant = ag.PolicyId.SqlIsNotNull(),
                    HasRestriction = fr.Policy.SqlIsNotNull(),
                };
        }

        return queryable;
    }

    public IQueryable<ProjectionWithAccessInfo<Category>> GetCategoriesWithAccessInfo(UserId? userId)
    {
        var timestamp = DateTime.UtcNow;
        IQueryable<ProjectionWithAccessInfo<Category>> queryable;
        if (userId == null)
        {
            queryable =
                from c in Categories
                from ap in Policies.Where(e => e.PolicyId == c.ReadPolicyId)
                select new ProjectionWithAccessInfo<Category>
                {
                    Projection = c,
                    ReadPolicyId = ap.PolicyId,
                    ReadPolicyValue = ap.Value,
                    HasGrant = false,
                    HasRestriction = false
                };
        }
        else
        {
            queryable =
                from c in Categories
                from ap in Policies.Where(e => e.PolicyId == c.ReadPolicyId)
                from ag in Grants
                    .Where(e => e.UserId == userId && e.PolicyId == c.ReadPolicyId)
                    .DefaultIfEmpty()
                from cr in CategoryRestrictions
                    .Where(e => e.UserId == userId && e.CategoryId == c.CategoryId &&
                                e.Policy == PolicyType.Read &&
                                (e.ExpiredAt == null || e.ExpiredAt > timestamp))
                    .DefaultIfEmpty()
                from fr in ForumRestrictions
                    .Where(e => e.UserId == userId && e.ForumId == c.ForumId &&
                                e.Policy == PolicyType.Read &&
                                (e.ExpiredAt == null || e.ExpiredAt > timestamp))
                    .DefaultIfEmpty()
                select new ProjectionWithAccessInfo<Category>
                {
                    Projection = c,
                    ReadPolicyId = ap.PolicyId,
                    ReadPolicyValue = ap.Value,
                    HasGrant = ag.PolicyId.SqlIsNotNull(),
                    HasRestriction = cr.Policy.SqlIsNotNull() || fr.Policy.SqlIsNotNull(),
                };
        }

        return queryable;
    }

    public IQueryable<ProjectionWithAccessInfo<Thread>> GetThreadsWithAccessInfo(UserId? userId)
    {
        var timestamp = DateTime.UtcNow;
        IQueryable<ProjectionWithAccessInfo<Thread>> queryable;
        if (userId == null)
        {
            queryable =
                from t in Threads
                from ap in Policies.Where(e => e.PolicyId == t.ReadPolicyId)
                select new ProjectionWithAccessInfo<Thread>
                {
                    Projection = t,
                    ReadPolicyId = ap.PolicyId,
                    ReadPolicyValue = ap.Value,
                    HasGrant = false,
                    HasRestriction = false
                };
        }
        else
        {
            queryable =
                from t in Threads
                from c in Categories.Where(e => e.CategoryId == t.CategoryId)
                from f in Forums.Where(e => e.ForumId == c.ForumId)
                from ap in Policies.Where(e => e.PolicyId == t.ReadPolicyId)
                from ag in Grants
                    .Where(e => e.UserId == userId && e.PolicyId == t.ReadPolicyId)
                    .DefaultIfEmpty()
                from tr in ThreadRestrictions
                    .Where(e => e.UserId == userId && e.ThreadId == t.ThreadId &&
                                e.Policy == PolicyType.Read &&
                                (e.ExpiredAt == null || e.ExpiredAt > timestamp))
                    .DefaultIfEmpty()
                from cr in CategoryRestrictions
                    .Where(e => e.UserId == userId && e.CategoryId == c.CategoryId &&
                                e.Policy == PolicyType.Read &&
                                (e.ExpiredAt == null || e.ExpiredAt > timestamp))
                    .DefaultIfEmpty()
                from fr in ForumRestrictions
                    .Where(e => e.UserId == userId && e.ForumId == f.ForumId &&
                                e.Policy == PolicyType.Read &&
                                (e.ExpiredAt == null || e.ExpiredAt > timestamp))
                    .DefaultIfEmpty()
                select new ProjectionWithAccessInfo<Thread>
                {
                    Projection = t,
                    ReadPolicyId = ap.PolicyId,
                    ReadPolicyValue = ap.Value,
                    HasGrant = ag.PolicyId.SqlIsNotNull(),
                    HasRestriction = tr.Policy.SqlIsNotNull() || cr.Policy.SqlIsNotNull() || fr.Policy.SqlIsNotNull(),
                };
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