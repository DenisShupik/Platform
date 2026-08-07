using CoreService.Domain.ValueObjects;
using Microsoft.EntityFrameworkCore;
using NotificationService.Domain.Entities;
using NotificationService.Domain.Enums;
using NotificationService.Domain.ValueObjects;
using NotificationService.Infrastructure.Persistence.Converters;
using Shared.Domain.Abstractions;
using Shared.Domain.ValueObjects;
using Shared.Infrastructure.Extensions;
using Shared.Infrastructure.Interfaces;
using Shared.Infrastructure.Persistence;

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
        configurationBuilder.ConfigureVogenValueObjects(
            typeof(NotifiableEventId).Assembly,
            typeof(ThreadId).Assembly,
            typeof(UserId).Assembly);
        configurationBuilder
            .Properties<EnumSet<ChannelType>>()
            .HaveConversion<EnumSetConverter<ChannelType>, EnumSetValueComparer<ChannelType>>();
    }

    public DbSet<ThreadSubscription> ThreadSubscriptions => Set<ThreadSubscription>();
    public DbSet<NotifiableEvent> NotifiableEvents => Set<NotifiableEvent>();
    public DbSet<Notification> Notifications => Set<Notification>();
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
