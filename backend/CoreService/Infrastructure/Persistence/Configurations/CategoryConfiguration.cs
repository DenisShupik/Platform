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
            .ValueGeneratedOnAdd();

        builder
            .Property(e => e.Title)
            .HasMaxLength(CategoryTitle.MaxLength);
        
        builder.HasIndex(e => e.Title);
    }
}