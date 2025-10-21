using CoreService.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace CoreService.Infrastructure.Persistence.Configurations;

public sealed class CategoryRestrictionConfiguration : IEntityTypeConfiguration<CategoryRestriction>
{
    public void Configure(EntityTypeBuilder<CategoryRestriction> builder)
    {
        builder.HasKey(e => new { e.UserId, e.CategoryId, e.Type });

        builder
            .Property(e => e.UserId)
            .ValueGeneratedNever();

        builder
            .Property(e => e.CategoryId)
            .ValueGeneratedNever();

        builder
            .HasOne<Category>()
            .WithMany()
            .HasForeignKey(e => e.CategoryId);
    }
}