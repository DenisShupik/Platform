using CoreService.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace CoreService.Infrastructure.Persistence.Configurations;

public sealed class ForumRestrictionConfiguration : IEntityTypeConfiguration<ForumRestriction>
{
    public void Configure(EntityTypeBuilder<ForumRestriction> builder)
    {
        builder.HasKey(e => new { e.UserId, e.ForumId, e.Type });

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