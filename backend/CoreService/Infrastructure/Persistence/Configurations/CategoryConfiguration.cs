using CoreService.Domain.Entities;
using CoreService.Domain.ValueObjects;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

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

        builder.OwnsOne(e => e.Policies);

        builder
            .HasOne<CategoryThreadAddable>()
            .WithOne()
            .HasForeignKey<CategoryThreadAddable>(e => e.CategoryId)
            .IsRequired();

        builder
            .HasOne<Forum>()
            .WithMany(e => e.Categories)
            .HasForeignKey(e => e.ForumId);

        builder
            .HasOne<ForumCategoryAddable>()
            .WithMany(e => e.Categories)
            .HasForeignKey(e => e.ForumId);
    }
}