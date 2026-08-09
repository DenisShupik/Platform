using CoreService.Domain.Entities;
using CoreService.Domain.ValueObjects;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using NpgsqlTypes;

namespace CoreService.Infrastructure.Persistence.Configurations;

public sealed class CategoryConfiguration : IEntityTypeConfiguration<Category>
{
    public void Configure(EntityTypeBuilder<Category> builder)
    {
        builder.HasKey(e => e.CategoryId);

        builder
            .Property(e => e.CategoryId)
            .ValueGeneratedNever();

        builder
            .Property(e => e.Title)
            .HasMaxLength(CategoryTitle.MaxLength);

        builder.HasIndex(e => e.Title);

        builder
            .Property<NpgsqlTsVector>(Constants.EnglishSearchVectorPropertyName)
            .HasColumnName(Constants.EnglishSearchVectorColumnName)
            .HasComputedColumnSql(
                $"to_tsvector('{Constants.EnglishTextSearchConfiguration}', coalesce(\"title\", ''))",
                stored: true);

        builder
            .HasIndex(Constants.EnglishSearchVectorPropertyName)
            .HasMethod("GIN");

        builder
            .Property<NpgsqlTsVector>(Constants.RussianSearchVectorPropertyName)
            .HasColumnName(Constants.RussianSearchVectorColumnName)
            .HasComputedColumnSql(
                $"to_tsvector('{Constants.RussianTextSearchConfiguration}', coalesce(\"title\", ''))",
                stored: true);

        builder
            .HasIndex(Constants.RussianSearchVectorPropertyName)
            .HasMethod("GIN");

        builder
            .HasOne<Forum>()
            .WithMany()
            .HasForeignKey(e => e.ForumId);
    }
}
