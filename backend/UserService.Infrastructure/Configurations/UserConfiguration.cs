using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using UserService.Domain.Entities;

namespace UserService.Infrastructure.Configurations;

public sealed class UserConfiguration:IEntityTypeConfiguration<User>
{
    public void Configure(EntityTypeBuilder<User> builder)
    {
        builder.HasKey(e => e.UserId);
    }
}