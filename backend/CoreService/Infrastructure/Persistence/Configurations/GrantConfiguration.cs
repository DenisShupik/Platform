using CoreService.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace CoreService.Infrastructure.Persistence.Configurations;

public sealed class GrantConfiguration : IEntityTypeConfiguration<Grant>
{
    public void Configure(EntityTypeBuilder<Grant> builder)
    {
        builder.HasKey(e => new { e.UserId, e.PolicyId });

        builder
            .Property(e => e.UserId)
            .ValueGeneratedNever();

        builder
            .Property(e => e.PolicyId)
            .ValueGeneratedNever();
    }
}