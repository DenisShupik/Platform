using CoreService.Domain.Entities;
using CoreService.Domain.ValueObjects;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Thread = CoreService.Domain.Entities.Thread;

namespace CoreService.Infrastructure.Persistence.Configurations;

public sealed class PostConfiguration : IEntityTypeConfiguration<Post>
{
    public void Configure(EntityTypeBuilder<Post> builder)
    {
        builder.HasKey(e => new { e.PostId, e.ThreadId });

        builder
            .Property(e => e.PostId)
            .ValueGeneratedNever();

        builder.HasIndex(e => e.ThreadId);

        builder
            .Property(e => e.Content)
            .HasMaxLength(PostContent.MaxLength);

        builder
            .Property(e => e.RowVersion)
            .IsRowVersion()
            ;

        builder
            .HasOne<Thread>()
            .WithMany(e => e.Posts)
            .HasForeignKey(e => e.ThreadId);

        builder
            .HasOne<ThreadPostAddable>()
            .WithMany(e => e.Posts)
            .HasForeignKey(e => e.ThreadId);
    }
}