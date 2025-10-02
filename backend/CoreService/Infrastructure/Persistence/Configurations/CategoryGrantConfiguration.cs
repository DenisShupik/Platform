using CoreService.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace CoreService.Infrastructure.Persistence.Configurations;

public sealed class CategoryGrantConfiguration : IEntityTypeConfiguration<CategoryGrant>
{
    public void Configure(EntityTypeBuilder<CategoryGrant> builder)
    {
        builder.HasKey(e => new { e.UserId, e.CategoryId, e.Policy });

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