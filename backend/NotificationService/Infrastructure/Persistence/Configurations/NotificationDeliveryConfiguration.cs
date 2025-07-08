using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using NotificationService.Domain.Entities;

namespace NotificationService.Infrastructure.Persistence.Configurations;

public sealed class NotificationDeliveryConfiguration : IEntityTypeConfiguration<UserNotification>
{
    public void Configure(EntityTypeBuilder<UserNotification> builder)
    {
        builder.HasKey(e => new { e.NotificationId, e.UserId, e.Channel });
        builder.HasIndex(e => e.NotificationId);
    }
}