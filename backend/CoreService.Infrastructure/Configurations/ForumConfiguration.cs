using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using CoreService.Domain.Entities;

namespace CoreService.Infrastructure.Configurations;

public sealed class ForumConfiguration : IEntityTypeConfiguration<Forum>
{
    public void Configure(EntityTypeBuilder<Forum> builder)
    {
        builder.HasKey(e => e.ForumId);

        builder
            .Property(e => e.ForumId)
            .ValueGeneratedOnAdd();

        builder
            .Property(e => e.Title)
            .HasMaxLength(Forum.TitleMaxLength);
    }
}