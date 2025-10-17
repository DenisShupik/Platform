using CoreService.Domain.Entities;
using CoreService.Domain.Enums;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace CoreService.Infrastructure.Persistence.Configurations;

public sealed class PolicyConfiguration : IEntityTypeConfiguration<Policy>
{
    public void Configure(EntityTypeBuilder<Policy> builder)
    {
        builder.HasKey(e => e.PolicyId);

        builder
            .Property(e => e.PolicyId)
            .ValueGeneratedNever();

        builder
            .HasDiscriminator(e => e.Type)
            .HasValue<ReadPolicy>(PolicyType.Read)
            .HasValue<ForumCreatePolicy>(PolicyType.ForumCreate)
            .HasValue<CategoryCreatePolicy>(PolicyType.CategoryCreate)
            .HasValue<ThreadCreatePolicy>(PolicyType.ThreadCreate)
            .HasValue<PostCreatePolicy>(PolicyType.PostCreate);
        
        builder
            .HasOne<Policy>()
            .WithMany(e => e.AddedPolicies)
            .HasForeignKey(e => e.ParentId)
            .IsRequired(false);
    }
}