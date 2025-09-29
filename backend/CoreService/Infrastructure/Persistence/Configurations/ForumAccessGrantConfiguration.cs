using CoreService.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace CoreService.Infrastructure.Persistence.Configurations;

public sealed class ForumAccessGrantConfiguration : IEntityTypeConfiguration<ForumAccessGrant>
{
    public void Configure(EntityTypeBuilder<ForumAccessGrant> builder)
    {
        builder.HasKey(e => new { e.UserId, e.ForumId });

        builder
            .Property(e => e.UserId)
            .ValueGeneratedNever();
        
        builder
            .Property(e => e.ForumId)
            .ValueGeneratedNever();

        builder
            .HasOne<Forum>()
            .WithMany()
            .HasForeignKey(e => e.ForumId);
    }
}