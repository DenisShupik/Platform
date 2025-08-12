using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using NotificationService.Domain.Entities;

namespace NotificationService.Infrastructure.Persistence.Configurations;

public sealed class NotificationConfiguration : IEntityTypeConfiguration<NotifiableEvent>
{
    public void Configure(EntityTypeBuilder<NotifiableEvent> builder)
    {
        builder.HasKey(e => e.NotifiableEventId);

        builder
            .Property(e => e.Payload)
            .HasColumnType("jsonb");
    }
}