using Microsoft.EntityFrameworkCore;
using NotificationService.Domain.Entities;
using NotificationService.Infrastructure.Persistence.Converters;
using SharedKernel.Infrastructure.Extensions;
using SharedKernel.Infrastructure.Interfaces;

namespace NotificationService.Infrastructure.Persistence;

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
        modelBuilder.ApplyTickerQConfiguration(Constants.DatabaseSchema);
    }

    protected override void ConfigureConventions(ModelConfigurationBuilder configurationBuilder)
    {
        base.ConfigureConventions(configurationBuilder);
        configurationBuilder.RegisterAllInVogenEfCoreConverters();
    }

    public DbSet<ThreadSubscription> ThreadSubscriptions => Set<ThreadSubscription>();
    public DbSet<NotifiableEvent> NotifiableEvents => Set<NotifiableEvent>();
    public DbSet<Notification> Notifications => Set<Notification>();
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