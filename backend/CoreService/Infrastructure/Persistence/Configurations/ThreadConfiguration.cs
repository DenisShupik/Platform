using CoreService.Domain.Entities;
using CoreService.Domain.ValueObjects;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using NpgsqlTypes;
using Thread = CoreService.Domain.Entities.Thread;

namespace CoreService.Infrastructure.Persistence.Configurations;

public sealed class ThreadConfiguration : IEntityTypeConfiguration<Thread>
{
    public void Configure(EntityTypeBuilder<Thread> builder)
    {
        builder.HasKey(e => e.ThreadId);

        builder
            .Property(e => e.ThreadId)
            .ValueGeneratedNever();

        builder
            .Property(e => e.Title)
            .HasMaxLength(ThreadTitle.MaxLength);

        builder
            .Property<NpgsqlTsVector>(Constants.SearchVectorPropertyName)
            .HasComputedColumnSql("to_tsvector('russian', coalesce(\"title\", ''))", stored: true);

        builder
            .HasIndex(Constants.SearchVectorPropertyName)
            .HasMethod("GIN");
        
        builder
            .HasOne<Category>()
            .WithMany()
            .HasForeignKey(e => e.CategoryId);
    }
}
