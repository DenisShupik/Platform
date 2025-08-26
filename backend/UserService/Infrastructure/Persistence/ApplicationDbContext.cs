using Microsoft.EntityFrameworkCore;
using SharedKernel.Infrastructure.Interfaces;
using UserService.Domain.Entities;
using UserService.Infrastructure.Persistence.Configurations;

namespace UserService.Infrastructure.Persistence;

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
    }

    protected override void ConfigureConventions(ModelConfigurationBuilder configurationBuilder)
    {
        base.ConfigureConventions(configurationBuilder);
        configurationBuilder.RegisterAllInVogenEfCoreConverters();
    }

    public DbSet<User> Users => Set<User>();
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