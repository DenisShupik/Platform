using CoreService.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Thread = CoreService.Domain.Entities.Thread;

namespace CoreService.Infrastructure.Persistence.Configurations;

// public sealed class AccessRestrictionConfiguration : IEntityTypeConfiguration<AccessRestriction>
// {
//     public void Configure(EntityTypeBuilder<AccessRestriction> builder)
//     {
//         builder.HasNoKey();
//         
//         builder.HasIndex(e => e.UserId);
//
//         builder
//             .Property(e => e.UserId)
//             .ValueGeneratedNever();
//     }
// }

public sealed class ForumAccessRestrictionConfiguration : IEntityTypeConfiguration<ForumAccessRestriction>
{
    public void Configure(EntityTypeBuilder<ForumAccessRestriction> builder)
    {
        builder.HasKey(e => new { e.UserId, e.ForumId });

        builder
            .Property(e => e.ForumId)
            .ValueGeneratedNever();

        builder
            .HasOne<Forum>()
            .WithMany()
            .HasForeignKey(e => e.ForumId);
    }
}

public sealed class CategoryRestrictionConfiguration : IEntityTypeConfiguration<CategoryAccessRestriction>
{
    public void Configure(EntityTypeBuilder<CategoryAccessRestriction> builder)
    {
        builder.HasKey(e => new { e.UserId, e.CategoryId });

        builder
            .Property(e => e.CategoryId)
            .ValueGeneratedNever();

        builder
            .HasOne<Category>()
            .WithMany()
            .HasForeignKey(e => e.CategoryId);
    }
}

public sealed class ThreadAccessRestrictionConfiguration : IEntityTypeConfiguration<ThreadAccessRestriction>
{
    public void Configure(EntityTypeBuilder<ThreadAccessRestriction> builder)
    {
        builder.HasKey(e => new { e.UserId, e.ThreadId });

        builder
            .Property(e => e.ThreadId)
            .ValueGeneratedNever();

        builder
            .HasOne<Thread>()
            .WithMany()
            .HasForeignKey(e => e.ThreadId);
    }
}