using CoreService.Domain.Entities;
using CoreService.Domain.ValueObjects;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Thread = CoreService.Domain.Entities.Thread;

namespace CoreService.Infrastructure.Persistence.Configurations;

public sealed class ThreadConfiguration : IEntityTypeConfiguration<Thread>
{
    public void Configure(EntityTypeBuilder<Thread> builder)
    {
        builder.HasKey(e => e.ThreadId);

        builder
            .Property(e => e.ThreadId)
            .ValueGeneratedNever();

        builder
            .Property(e => e.Title)
            .HasMaxLength(ThreadTitle.MaxLength);

        builder.OwnsOne(e => e.Policies);

        builder
            .HasOne<ThreadPostAddable>()
            .WithOne()
            .HasForeignKey<ThreadPostAddable>(e => e.ThreadId)
            .IsRequired();

        builder
            .HasOne<Category>()
            .WithMany(e => e.Threads)
            .HasForeignKey(e => e.CategoryId);

        builder
            .HasOne<CategoryThreadAddable>()
            .WithMany(e => e.Threads)
            .HasForeignKey(e => e.CategoryId);
    }
}