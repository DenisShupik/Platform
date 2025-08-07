using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using UserService.Domain.Entities;
using UserService.Domain.ValueObjects;

namespace UserService.Infrastructure.Persistence.Configurations;

public sealed class UserConfiguration : IEntityTypeConfiguration<User>
{
    public void Configure(EntityTypeBuilder<User> builder)
    {
        builder.HasKey(e => e.UserId);

        builder
            .Property(e => e.UserId)
            .ValueGeneratedNever();

        builder
            .Property(e => e.Username)
            .HasMaxLength(Username.MaxLength);
    }
}