using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using TopicService.Domain.Entities;

namespace TopicService.Infrastructure.Configurations;

public sealed class PostConfiguration : IEntityTypeConfiguration<Post>
{
    public void Configure(EntityTypeBuilder<Post> builder)
    {
        builder.HasKey(e => e.PostId);

        builder
            .Property(e => e.PostId)
            .ValueGeneratedOnAdd();

        builder.HasIndex(e => e.TopicId);

        builder
            .Property(e => e.Content)
            .HasMaxLength(Post.ContentMaxLength);
    }
}