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
            .Property<NpgsqlTsVector>(Constants.SearchVectorPropertyName)
            .HasComputedColumnSql("to_tsvector('russian', coalesce(\"title\", ''))", stored: true);

        builder
            .HasIndex(Constants.SearchVectorPropertyName)
            .HasMethod("GIN");
        
        builder
            .HasOne<Forum>()
            .WithMany()
            .HasForeignKey(e => e.ForumId);
    }
}
