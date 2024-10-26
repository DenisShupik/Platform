using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using TopicService.Domain.Entities;

namespace TopicService.Infrastructure.Configurations;

public sealed class TopicConfiguration : IEntityTypeConfiguration<Topic>
{
    public void Configure(EntityTypeBuilder<Topic> builder)
    {
        builder.HasKey(e => e.TopicId);

        builder
            .Property(e => e.TopicId)
            .ValueGeneratedOnAdd();
    }
}