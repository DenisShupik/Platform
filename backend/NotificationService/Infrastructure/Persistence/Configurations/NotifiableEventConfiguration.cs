using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using NotificationService.Domain.Entities;

namespace NotificationService.Infrastructure.Persistence.Configurations;

public sealed class NotifiableEventConfiguration : IEntityTypeConfiguration<NotifiableEvent>
{
    public void Configure(EntityTypeBuilder<NotifiableEvent> builder)
    {
        builder.HasKey(e => e.NotifiableEventId);

        builder
            .Property(e => e.NotifiableEventId)
            .ValueGeneratedNever();
        
        builder
            .Property(e => e.Payload)
            .HasColumnType("jsonb");
    }
}