using CoreService.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Thread = CoreService.Domain.Entities.Thread;

namespace CoreService.Infrastructure.Persistence.Configurations;

public sealed class ThreadAccessGrantConfiguration : IEntityTypeConfiguration<ThreadAccessGrant>
{
    public void Configure(EntityTypeBuilder<ThreadAccessGrant> builder)
    {
        builder.HasKey(e => new { e.UserId, e.ThreadId });

        builder
            .Property(e => e.UserId)
            .ValueGeneratedNever();

        builder
            .Property(e => e.ThreadId)
            .ValueGeneratedNever();

        builder
            .HasOne<Thread>()
            .WithMany()
            .HasForeignKey(e => e.ThreadId);
    }
}