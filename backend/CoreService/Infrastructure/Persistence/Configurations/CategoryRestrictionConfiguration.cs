using CoreService.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace CoreService.Infrastructure.Persistence.Configurations;

public sealed class CategoryRestrictionConfiguration : IEntityTypeConfiguration<CategoryAccessRestriction>
{
    public void Configure(EntityTypeBuilder<CategoryAccessRestriction> builder)
    {
        builder.HasKey(e => new { e.UserId, e.CategoryId });

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