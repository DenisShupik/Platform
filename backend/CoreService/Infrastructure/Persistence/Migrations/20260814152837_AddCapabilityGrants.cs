using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace CoreService.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class AddCapabilityGrants : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "capability_grants",
                schema: "core_service",
                columns: table => new
                {
                    capability_grant_id = table.Column<Guid>(type: "uuid", nullable: false),
                    assignment_id = table.Column<Guid>(type: "uuid", nullable: false),
                    user_id = table.Column<Guid>(type: "uuid", nullable: false),
                    capability = table.Column<short>(type: "smallint", nullable: false),
                    scope_type = table.Column<byte>(type: "smallint", nullable: false),
                    forum_id = table.Column<Guid>(type: "uuid", nullable: true),
                    category_id = table.Column<Guid>(type: "uuid", nullable: true),
                    source_type = table.Column<byte>(type: "smallint", nullable: false),
                    granted_by = table.Column<Guid>(type: "uuid", nullable: true),
                    granted_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    valid_until = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    revoked_by = table.Column<Guid>(type: "uuid", nullable: true),
                    revoked_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_capability_grants", x => x.capability_grant_id);
                    table.CheckConstraint("CK_capability_grants_capability_Enum", "capability BETWEEN 1 AND 7");
                    table.CheckConstraint("ck_capability_grants_issuer", "(source_type = 3 AND granted_by IS NULL) OR (source_type <> 3 AND granted_by IS NOT NULL)");
                    table.CheckConstraint("ck_capability_grants_revocation", "(revoked_at IS NULL AND revoked_by IS NULL) OR (revoked_at IS NOT NULL AND revoked_by IS NOT NULL)");
                    table.CheckConstraint("ck_capability_grants_scope", "(scope_type = 1 AND forum_id IS NULL AND category_id IS NULL) OR (scope_type = 2 AND forum_id IS NOT NULL AND category_id IS NULL) OR (scope_type = 3 AND forum_id IS NOT NULL AND category_id IS NOT NULL)");
                    table.CheckConstraint("CK_capability_grants_scope_type_Enum", "scope_type BETWEEN 1 AND 3");
                    table.CheckConstraint("CK_capability_grants_source_type_Enum", "source_type BETWEEN 1 AND 4");
                    table.CheckConstraint("ck_capability_grants_validity", "valid_until IS NULL OR valid_until > granted_at");
                    table.ForeignKey(
                        name: "fk_capability_grants_categories_category_id",
                        column: x => x.category_id,
                        principalSchema: "core_service",
                        principalTable: "categories",
                        principalColumn: "category_id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "fk_capability_grants_forums_forum_id",
                        column: x => x.forum_id,
                        principalSchema: "core_service",
                        principalTable: "forums",
                        principalColumn: "forum_id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "ix_capability_grants_assignment_id",
                schema: "core_service",
                table: "capability_grants",
                column: "assignment_id");

            migrationBuilder.CreateIndex(
                name: "ix_capability_grants_category_id",
                schema: "core_service",
                table: "capability_grants",
                column: "category_id");

            migrationBuilder.CreateIndex(
                name: "ix_capability_grants_forum_id",
                schema: "core_service",
                table: "capability_grants",
                column: "forum_id");

            migrationBuilder.CreateIndex(
                name: "ix_capability_grants_user_id_capability_scope_type_forum_id_ca",
                schema: "core_service",
                table: "capability_grants",
                columns: new[] { "user_id", "capability", "scope_type", "forum_id", "category_id", "revoked_at", "valid_until" });

            migrationBuilder.CreateIndex(
                name: "ix_capability_grants_user_id_source_type_capability",
                schema: "core_service",
                table: "capability_grants",
                columns: new[] { "user_id", "source_type", "capability" },
                unique: true,
                filter: "source_type IN (3, 4) AND revoked_at IS NULL");

            migrationBuilder.CreateIndex(
                name: "ix_capability_grants_user_id_source_type_category_id_capability",
                schema: "core_service",
                table: "capability_grants",
                columns: new[] { "user_id", "source_type", "category_id", "capability" },
                unique: true,
                filter: "source_type = 2 AND revoked_at IS NULL");

            migrationBuilder.CreateIndex(
                name: "ix_capability_grants_user_id_source_type_category_id_revoked_at",
                schema: "core_service",
                table: "capability_grants",
                columns: new[] { "user_id", "source_type", "category_id", "revoked_at" });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "capability_grants",
                schema: "core_service");
        }
    }
}
