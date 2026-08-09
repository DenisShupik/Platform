using System.Globalization;
using CoreService.Domain.ValueObjects;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using NotificationService.Domain.Entities;
using NotificationService.Infrastructure.Persistence.Converters;
using Npgsql.NameTranslation;

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
            .HasConversion<NotifiableEventPayloadValueConverter>()
            .HasColumnType("jsonb");

        var payloadColumn = NpgsqlSnakeCaseNameTranslator.ConvertToSnakeCase(
            nameof(NotifiableEvent.Payload),
            CultureInfo.InvariantCulture);
        var postIdProperty = nameof(IPostNotifiableEventPayload.PostId);
        var threadIdProperty = nameof(IPostNotifiableEventPayload.ThreadId);

        builder
            .Property<ThreadId?>(threadIdProperty)
            .HasComputedColumnSql(
                $"CASE WHEN \"{payloadColumn}\" ? '{postIdProperty}' " +
                $"THEN (\"{payloadColumn}\"->>'{threadIdProperty}')::uuid END",
                stored: true);

        builder
            .HasIndex(
                threadIdProperty,
                nameof(NotifiableEvent.OccurredAt),
                nameof(NotifiableEvent.NotifiableEventId))
            .HasDatabaseName("ix_notifiable_events_post_thread_latest")
            .IsDescending(false, true, true);
    }
}
