using CoreService.Domain.Entities;
using CoreService.Domain.ValueObjects;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

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
            .HasOne<Policy>()
            .WithMany()
            .HasForeignKey(e => e.AccessPolicyId);
        
        builder
            .HasOne<Policy>()
            .WithMany()
            .HasForeignKey(e => e.CategoryCreatePolicyId);
        
        builder
            .HasOne<Policy>()
            .WithMany()
            .HasForeignKey(e => e.ThreadCreatePolicyId);
        
        builder
            .HasOne<Policy>()
            .WithMany()
            .HasForeignKey(e => e.PostCreatePolicyId);

        builder
            .HasOne<ForumCategoryAddable>()
            .WithOne()
            .HasForeignKey<ForumCategoryAddable>(e => e.ForumId)
            .IsRequired();
    }
}