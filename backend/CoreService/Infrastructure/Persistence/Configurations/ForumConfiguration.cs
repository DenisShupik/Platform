using CoreService.Domain.Entities;
using CoreService.Domain.ValueObjects;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using NpgsqlTypes;

namespace CoreService.Infrastructure.Persistence.Configurations;

public sealed class ForumConfiguration : IEntityTypeConfiguration<Forum>
{
    public void Configure(EntityTypeBuilder<Forum> builder)
    {
        builder.HasKey(e => e.ForumId);

        builder
            .Property(e => e.ForumId)
            .ValueGeneratedNever();

        builder
            .Property(e => e.Title)
            .HasMaxLength(ForumTitle.MaxLength);

        builder.HasIndex(e => e.Title);

        builder
            .Property<NpgsqlTsVector>(Constants.SearchVectorPropertyName)
            .HasComputedColumnSql("to_tsvector('russian', coalesce(\"title\", ''))", stored: true);

        builder
            .HasIndex(Constants.SearchVectorPropertyName)
            .HasMethod("GIN");
    }
}
