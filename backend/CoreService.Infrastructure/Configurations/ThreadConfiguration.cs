using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using CoreService.Domain.Entities;
using Thread = CoreService.Domain.Entities.Thread;

namespace CoreService.Infrastructure.Configurations;

public sealed class ThreadConfiguration : IEntityTypeConfiguration<Thread>
{
    public void Configure(EntityTypeBuilder<Thread> builder)
    {
        builder.HasKey(e => e.ThreadId);

        builder
            .Property(e => e.ThreadId)
            .ValueGeneratedOnAdd();

        builder
            .Property(e => e.Title)
            .HasMaxLength(Thread.TitleMaxLength);
    }
}