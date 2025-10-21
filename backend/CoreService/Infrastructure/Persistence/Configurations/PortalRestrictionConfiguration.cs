using CoreService.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace CoreService.Infrastructure.Persistence.Configurations;

public sealed class PortalRestrictionConfiguration : IEntityTypeConfiguration<PortalRestriction>
{
    public void Configure(EntityTypeBuilder<PortalRestriction> builder)
    {
        builder.HasKey(e => new { e.UserId, e.Type });

        builder
            .Property(e => e.UserId)
            .ValueGeneratedNever();
    }
}