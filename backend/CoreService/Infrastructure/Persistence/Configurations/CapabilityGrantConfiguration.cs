using System.Globalization;
using System.Linq.Expressions;
using System.Reflection;
using CoreService.Domain.Entities;
using CoreService.Domain.Enums;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Npgsql.NameTranslation;

namespace CoreService.Infrastructure.Persistence.Configurations;

public sealed class CapabilityGrantConfiguration : IEntityTypeConfiguration<CapabilityGrant>
{
    private const string TableName = "capability_grants";
    private static readonly NpgsqlSnakeCaseNameTranslator NameTranslator = new();

    public void Configure(EntityTypeBuilder<CapabilityGrant> builder)
    {
        var scopeTypeColumn = MapColumn(builder, e => e.ScopeType);
        var forumIdColumn = MapColumn(builder, e => e.ForumId);
        var categoryIdColumn = MapColumn(builder, e => e.CategoryId);
        var threadIdColumn = MapColumn(builder, e => e.ThreadId);
        var sourceTypeColumn = MapColumn(builder, e => e.SourceType);
        var grantedAtColumn = MapColumn(builder, e => e.GrantedAt);
        var validUntilColumn = MapColumn(builder, e => e.ValidUntil);
        var revokedByColumn = MapColumn(builder, e => e.RevokedBy);
        var revokedAtColumn = MapColumn(builder, e => e.RevokedAt);
        var grantedByColumn = MapColumn(builder, e => e.GrantedBy);

        builder.HasKey(e => e.CapabilityGrantId);
        builder.Property(e => e.CapabilityGrantId).ValueGeneratedNever();

        builder.HasIndex(e => e.AssignmentId);
        builder.HasIndex(e => new
        {
            e.UserId,
            e.Capability,
            e.ScopeType,
            e.ForumId,
            e.CategoryId,
            e.ThreadId,
            e.RevokedAt,
            e.ValidUntil
        });
        builder.HasIndex(e => new { e.UserId, e.SourceType, e.CategoryId, e.RevokedAt });
        builder.HasIndex(e => new { e.UserId, e.SourceType, e.ForumId, e.RevokedAt });
        builder.HasIndex(e => new { e.UserId, e.SourceType, e.Capability, e.ForumId })
            .IsUnique()
            .HasFilter(
                $"{sourceTypeColumn} = {ToNumber(GrantSourceType.Direct)} " +
                $"AND {scopeTypeColumn} = {ToNumber(AuthorizationScopeType.Forum)} " +
                $"AND {revokedAtColumn} IS NULL");
        builder.HasIndex(e => new { e.UserId, e.SourceType, e.Capability, e.CategoryId })
            .IsUnique()
            .HasFilter(
                $"{sourceTypeColumn} = {ToNumber(GrantSourceType.Direct)} " +
                $"AND {scopeTypeColumn} = {ToNumber(AuthorizationScopeType.Category)} " +
                $"AND {revokedAtColumn} IS NULL");
        builder.HasIndex(e => new { e.UserId, e.SourceType, e.Capability, e.ThreadId })
            .IsUnique()
            .HasFilter(
                $"{sourceTypeColumn} = {ToNumber(GrantSourceType.Direct)} " +
                $"AND {scopeTypeColumn} = {ToNumber(AuthorizationScopeType.Thread)} " +
                $"AND {revokedAtColumn} IS NULL");
        builder.HasIndex(e => new { e.UserId, e.SourceType, e.CategoryId, e.Capability })
            .IsUnique()
            .HasFilter(
                $"{sourceTypeColumn} = {ToNumber(GrantSourceType.CategoryModeratorAppointment)} " +
                $"AND {revokedAtColumn} IS NULL");
        builder.HasIndex(e => new { e.UserId, e.SourceType, e.ForumId, e.Capability })
            .IsUnique()
            .HasFilter(
                $"{sourceTypeColumn} = {ToNumber(GrantSourceType.ForumModeratorAppointment)} " +
                $"AND {revokedAtColumn} IS NULL");
        builder.HasIndex(e => new { e.UserId, e.SourceType, e.Capability })
            .IsUnique()
            .HasFilter(
                $"({sourceTypeColumn} IN ({ToNumber(GrantSourceType.PlatformAdministratorBootstrap)}, " +
                $"{ToNumber(GrantSourceType.PlatformAdministratorAppointment)}) OR " +
                $"({sourceTypeColumn} = {ToNumber(GrantSourceType.Direct)} " +
                $"AND {scopeTypeColumn} = {ToNumber(AuthorizationScopeType.Platform)})) " +
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
                $"{validUntilColumn} IS NULL OR {validUntilColumn} > {grantedAtColumn}");
            table.HasCheckConstraint(
                $"ck_{TableName}_revocation",
                $"({revokedAtColumn} IS NULL AND {revokedByColumn} IS NULL) OR " +
                $"({revokedAtColumn} IS NOT NULL AND {revokedByColumn} IS NOT NULL)");
            table.HasCheckConstraint(
                $"ck_{TableName}_issuer",
                $"({sourceTypeColumn} = {ToNumber(GrantSourceType.PlatformAdministratorBootstrap)} " +
                $"AND {grantedByColumn} IS NULL) OR " +
                $"({sourceTypeColumn} <> {ToNumber(GrantSourceType.PlatformAdministratorBootstrap)} " +
                $"AND {grantedByColumn} IS NOT NULL)");
        });

        builder.HasOne<Forum>()
            .WithMany()
            .HasForeignKey(e => e.ForumId)
            .OnDelete(DeleteBehavior.Cascade);
        builder.HasOne<Category>()
            .WithMany()
            .HasForeignKey(e => e.CategoryId)
            .OnDelete(DeleteBehavior.Cascade);
        builder.HasOne<CoreService.Domain.Entities.Thread>()
            .WithMany()
            .HasForeignKey(e => e.ThreadId)
            .OnDelete(DeleteBehavior.Cascade);
    }

    private static string MapColumn<TProperty>(
        EntityTypeBuilder<CapabilityGrant> builder,
        Expression<Func<CapabilityGrant, TProperty>> propertyExpression)
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
