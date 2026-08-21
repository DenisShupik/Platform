using System.Globalization;
using System.Linq.Expressions;
using System.Reflection;
using CoreService.Domain.Entities;
using CoreService.Domain.Enums;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Npgsql.NameTranslation;

namespace CoreService.Infrastructure.Persistence.Configurations;

public sealed class ForumSanctionConfiguration : IEntityTypeConfiguration<ForumSanction>
{
    private const string TableName = "forum_sanctions";
    private static readonly NpgsqlSnakeCaseNameTranslator NameTranslator = new();

    public void Configure(EntityTypeBuilder<ForumSanction> builder)
    {
        MapColumn(builder, sanction => sanction.Type);
        var scopeTypeColumn = MapColumn(builder, sanction => sanction.ScopeType);
        var forumIdColumn = MapColumn(builder, sanction => sanction.ForumId);
        var categoryIdColumn = MapColumn(builder, sanction => sanction.CategoryId);
        var threadIdColumn = MapColumn(builder, sanction => sanction.ThreadId);
        var issuedAtColumn = MapColumn(builder, sanction => sanction.IssuedAt);
        var validUntilColumn = MapColumn(builder, sanction => sanction.ValidUntil);
        var revokedByColumn = MapColumn(builder, sanction => sanction.RevokedBy);
        var revokedAtColumn = MapColumn(builder, sanction => sanction.RevokedAt);

        builder.HasKey(sanction => sanction.ForumSanctionId);
        builder.Property(sanction => sanction.ForumSanctionId).ValueGeneratedNever();
        builder.Property(sanction => sanction.Reason).HasMaxLength(500);

        builder.HasIndex(sanction => new
        {
            sanction.UserId,
            sanction.ScopeType,
            sanction.ForumId,
            sanction.CategoryId,
            sanction.ThreadId,
            sanction.RevokedAt,
            sanction.ValidUntil
        });
        builder.HasIndex(sanction => new { sanction.UserId, sanction.Type })
            .IsUnique()
            .HasFilter(
                $"{scopeTypeColumn} = {ToNumber(AuthorizationScopeType.Platform)} " +
                $"AND {revokedAtColumn} IS NULL");
        builder.HasIndex(sanction => new { sanction.UserId, sanction.Type, sanction.ForumId })
            .IsUnique()
            .HasFilter(
                $"{scopeTypeColumn} = {ToNumber(AuthorizationScopeType.Forum)} " +
                $"AND {revokedAtColumn} IS NULL");
        builder.HasIndex(sanction => new { sanction.UserId, sanction.Type, sanction.CategoryId })
            .IsUnique()
            .HasFilter(
                $"{scopeTypeColumn} = {ToNumber(AuthorizationScopeType.Category)} " +
                $"AND {revokedAtColumn} IS NULL");
        builder.HasIndex(sanction => new { sanction.UserId, sanction.Type, sanction.ThreadId })
            .IsUnique()
            .HasFilter(
                $"{scopeTypeColumn} = {ToNumber(AuthorizationScopeType.Thread)} " +
                $"AND {revokedAtColumn} IS NULL");

        builder.ToTable(TableName, table =>
        {
            table.HasCheckConstraint(
                $"ck_{TableName}_scope",
                $"({scopeTypeColumn} = {ToNumber(AuthorizationScopeType.Platform)} " +
                $"AND {forumIdColumn} IS NULL AND {categoryIdColumn} IS NULL AND {threadIdColumn} IS NULL) OR " +
                $"({scopeTypeColumn} = {ToNumber(AuthorizationScopeType.Forum)} " +
                $"AND {forumIdColumn} IS NOT NULL AND {categoryIdColumn} IS NULL AND {threadIdColumn} IS NULL) OR " +
                $"({scopeTypeColumn} = {ToNumber(AuthorizationScopeType.Category)} " +
                $"AND {forumIdColumn} IS NOT NULL AND {categoryIdColumn} IS NOT NULL AND {threadIdColumn} IS NULL) OR " +
                $"({scopeTypeColumn} = {ToNumber(AuthorizationScopeType.Thread)} " +
                $"AND {forumIdColumn} IS NOT NULL AND {categoryIdColumn} IS NOT NULL AND {threadIdColumn} IS NOT NULL)");
            table.HasCheckConstraint(
                $"ck_{TableName}_validity",
                $"{validUntilColumn} IS NULL OR {validUntilColumn} > {issuedAtColumn}");
            table.HasCheckConstraint(
                $"ck_{TableName}_revocation",
                $"({revokedAtColumn} IS NULL AND {revokedByColumn} IS NULL) OR " +
                $"({revokedAtColumn} IS NOT NULL AND {revokedByColumn} IS NOT NULL)");
        });

        builder.HasOne<Forum>()
            .WithMany()
            .HasForeignKey(sanction => sanction.ForumId)
            .OnDelete(DeleteBehavior.Cascade);
        builder.HasOne<Category>()
            .WithMany()
            .HasForeignKey(sanction => sanction.CategoryId)
            .OnDelete(DeleteBehavior.Cascade);
        builder.HasOne<CoreService.Domain.Entities.Thread>()
            .WithMany()
            .HasForeignKey(sanction => sanction.ThreadId)
            .OnDelete(DeleteBehavior.Cascade);
    }

    private static string MapColumn<TProperty>(
        EntityTypeBuilder<ForumSanction> builder,
        Expression<Func<ForumSanction, TProperty>> propertyExpression)
    {
        if (propertyExpression.Body is not MemberExpression { Member: PropertyInfo property })
            throw new ArgumentException("A direct property expression is required.", nameof(propertyExpression));

        var columnName = NameTranslator.TranslateMemberName(property.Name);
        builder.Property(propertyExpression).HasColumnName(columnName);
        return columnName;
    }

    private static string ToNumber<TEnum>(TEnum value) where TEnum : struct, Enum =>
        Convert.ToInt64(value, CultureInfo.InvariantCulture).ToString(CultureInfo.InvariantCulture);
}
