using CoreService.Domain.Entities;
using CoreService.Domain.ValueObjects;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using NpgsqlTypes;
using Thread = CoreService.Domain.Entities.Thread;

namespace CoreService.Infrastructure.Persistence.Configurations;

public sealed class PostConfiguration : IEntityTypeConfiguration<Post>
{
    public void Configure(EntityTypeBuilder<Post> builder)
    {
        builder.HasKey(e => e.PostId);

        builder
            .Property(e => e.PostId)
            .ValueGeneratedNever();

        builder.HasIndex(e => e.ThreadId);
        builder.HasIndex(e => new { e.ThreadId, e.CreatedAt, e.PostId });

        builder
            .Property(e => e.Content)
            .HasMaxLength(PostContent.MaxLength);

        builder
            .Property<string>(Constants.SearchTextPropertyName)
            .HasMaxLength(PostContent.MaxLength)
            .IsRequired();

        builder
            .Property<NpgsqlTsVector>(Constants.EnglishSearchVectorPropertyName)
            .HasColumnName(Constants.EnglishSearchVectorColumnName)
            .HasComputedColumnSql(
                $"to_tsvector('{Constants.EnglishTextSearchConfiguration}', coalesce(\"{Constants.SearchTextColumnName}\", ''))",
                stored: true);

        builder
            .HasIndex(Constants.EnglishSearchVectorPropertyName)
            .HasMethod("GIN");

        builder
            .Property<NpgsqlTsVector>(Constants.RussianSearchVectorPropertyName)
            .HasColumnName(Constants.RussianSearchVectorColumnName)
            .HasComputedColumnSql(
                $"to_tsvector('{Constants.RussianTextSearchConfiguration}', coalesce(\"{Constants.SearchTextColumnName}\", ''))",
                stored: true);

        builder
            .HasIndex(Constants.RussianSearchVectorPropertyName)
            .HasMethod("GIN");

        builder
            .Property(e => e.RowVersion)
            .IsRowVersion();

        builder
            .HasOne<Thread>()
            .WithMany()
            .HasForeignKey(e => e.ThreadId);
    }
}
