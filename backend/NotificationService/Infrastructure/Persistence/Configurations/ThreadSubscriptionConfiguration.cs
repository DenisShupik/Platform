using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using NotificationService.Domain.Entities;

namespace NotificationService.Infrastructure.Persistence.Configurations;

public sealed class ThreadSubscriptionConfiguration : IEntityTypeConfiguration<ThreadSubscription>
{
    public void Configure(EntityTypeBuilder<ThreadSubscription> builder)
    {
        builder.HasKey(e => new { e.UserId, e.ThreadId });
        builder.HasIndex(e => e.UserId);
        builder.HasIndex(e => e.ThreadId);
    }
}