using Microsoft.EntityFrameworkCore;
using CoreService.Domain.Entities;
using CoreService.Domain.ValueObjects;
using CoreService.Infrastructure.Persistence.Configurations;
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
    public DbSet<Post> Posts => Set<Post>();
}