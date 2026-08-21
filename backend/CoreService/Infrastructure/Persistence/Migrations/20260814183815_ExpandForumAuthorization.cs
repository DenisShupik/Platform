using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace CoreService.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class ExpandForumAuthorization : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "ix_capability_grants_user_id_capability_scope_type_forum_id_ca",
                schema: "core_service",
                table: "capability_grants");

            migrationBuilder.DropIndex(
                name: "ix_capability_grants_user_id_source_type_capability",
                schema: "core_service",
                table: "capability_grants");

            migrationBuilder.DropCheckConstraint(
                name: "CK_capability_grants_capability_Enum",
                schema: "core_service",
                table: "capability_grants");

            migrationBuilder.DropCheckConstraint(
                name: "ck_capability_grants_scope",
                schema: "core_service",
                table: "capability_grants");

            migrationBuilder.DropCheckConstraint(
                name: "CK_capability_grants_scope_type_Enum",
                schema: "core_service",
                table: "capability_grants");

            migrationBuilder.DropCheckConstraint(
                name: "CK_capability_grants_source_type_Enum",
                schema: "core_service",
                table: "capability_grants");

            migrationBuilder.AddColumn<Guid>(
                name: "thread_id",
                schema: "core_service",
                table: "capability_grants",
                type: "uuid",
                nullable: true);

            migrationBuilder.CreateTable(
                name: "forum_sanctions",
                schema: "core_service",
                columns: table => new
                {
                    forum_sanction_id = table.Column<Guid>(type: "uuid", nullable: false),
                    user_id = table.Column<Guid>(type: "uuid", nullable: false),
                    type = table.Column<byte>(type: "smallint", nullable: false),
                    scope_type = table.Column<byte>(type: "smallint", nullable: false),
                    forum_id = table.Column<Guid>(type: "uuid", nullable: true),
                    category_id = table.Column<Guid>(type: "uuid", nullable: true),
                    thread_id = table.Column<Guid>(type: "uuid", nullable: true),
                    reason = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: false),
                    issued_by = table.Column<Guid>(type: "uuid", nullable: false),
                    issued_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    valid_until = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    revoked_by = table.Column<Guid>(type: "uuid", nullable: true),
                    revoked_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_forum_sanctions", x => x.forum_sanction_id);
                    table.CheckConstraint("ck_forum_sanctions_revocation", "(revoked_at IS NULL AND revoked_by IS NULL) OR (revoked_at IS NOT NULL AND revoked_by IS NOT NULL)");
                    table.CheckConstraint("ck_forum_sanctions_scope", "(scope_type = 1 AND forum_id IS NULL AND category_id IS NULL AND thread_id IS NULL) OR (scope_type = 2 AND forum_id IS NOT NULL AND category_id IS NULL AND thread_id IS NULL) OR (scope_type = 3 AND forum_id IS NOT NULL AND category_id IS NOT NULL AND thread_id IS NULL) OR (scope_type = 4 AND forum_id IS NOT NULL AND category_id IS NOT NULL AND thread_id IS NOT NULL)");
                    table.CheckConstraint("CK_forum_sanctions_scope_type_Enum", "scope_type BETWEEN 1 AND 4");
                    table.CheckConstraint("CK_forum_sanctions_type_Enum", "type IN (1, 2)");
                    table.CheckConstraint("ck_forum_sanctions_validity", "valid_until IS NULL OR valid_until > issued_at");
                    table.ForeignKey(
                        name: "fk_forum_sanctions_categories_category_id",
                        column: x => x.category_id,
                        principalSchema: "core_service",
                        principalTable: "categories",
                        principalColumn: "category_id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "fk_forum_sanctions_forums_forum_id",
                        column: x => x.forum_id,
                        principalSchema: "core_service",
                        principalTable: "forums",
                        principalColumn: "forum_id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "fk_forum_sanctions_threads_thread_id",
                        column: x => x.thread_id,
                        principalSchema: "core_service",
                        principalTable: "threads",
                        principalColumn: "thread_id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "ix_capability_grants_thread_id",
                schema: "core_service",
                table: "capability_grants",
                column: "thread_id");

            migrationBuilder.CreateIndex(
                name: "ix_capability_grants_user_id_capability_scope_type_forum_id_ca",
                schema: "core_service",
                table: "capability_grants",
                columns: new[] { "user_id", "capability", "scope_type", "forum_id", "category_id", "thread_id", "revoked_at", "valid_until" });

            migrationBuilder.CreateIndex(
                name: "ix_capability_grants_user_id_source_type_capability",
                schema: "core_service",
                table: "capability_grants",
                columns: new[] { "user_id", "source_type", "capability" },
                unique: true,
                filter: "(source_type IN (3, 4) OR (source_type = 1 AND scope_type = 1)) AND revoked_at IS NULL");

            migrationBuilder.CreateIndex(
                name: "ix_capability_grants_user_id_source_type_capability_category_id",
                schema: "core_service",
                table: "capability_grants",
                columns: new[] { "user_id", "source_type", "capability", "category_id" },
                unique: true,
                filter: "source_type = 1 AND scope_type = 3 AND revoked_at IS NULL");

            migrationBuilder.CreateIndex(
                name: "ix_capability_grants_user_id_source_type_capability_forum_id",
                schema: "core_service",
                table: "capability_grants",
                columns: new[] { "user_id", "source_type", "capability", "forum_id" },
                unique: true,
                filter: "source_type = 1 AND scope_type = 2 AND revoked_at IS NULL");

            migrationBuilder.CreateIndex(
                name: "ix_capability_grants_user_id_source_type_capability_thread_id",
                schema: "core_service",
                table: "capability_grants",
                columns: new[] { "user_id", "source_type", "capability", "thread_id" },
                unique: true,
                filter: "source_type = 1 AND scope_type = 4 AND revoked_at IS NULL");

            migrationBuilder.CreateIndex(
                name: "ix_capability_grants_user_id_source_type_forum_id_capability",
                schema: "core_service",
                table: "capability_grants",
                columns: new[] { "user_id", "source_type", "forum_id", "capability" },
                unique: true,
                filter: "source_type = 5 AND revoked_at IS NULL");

            migrationBuilder.CreateIndex(
                name: "ix_capability_grants_user_id_source_type_forum_id_revoked_at",
                schema: "core_service",
                table: "capability_grants",
                columns: new[] { "user_id", "source_type", "forum_id", "revoked_at" });

            migrationBuilder.AddCheckConstraint(
                name: "CK_capability_grants_capability_Enum",
                schema: "core_service",
                table: "capability_grants",
                sql: "capability BETWEEN 1 AND 8");

            migrationBuilder.AddCheckConstraint(
                name: "ck_capability_grants_scope",
                schema: "core_service",
                table: "capability_grants",
                sql: "(scope_type = 1 AND forum_id IS NULL AND category_id IS NULL AND thread_id IS NULL) OR (scope_type = 2 AND forum_id IS NOT NULL AND category_id IS NULL AND thread_id IS NULL) OR (scope_type = 3 AND forum_id IS NOT NULL AND category_id IS NOT NULL AND thread_id IS NULL) OR (scope_type = 4 AND forum_id IS NOT NULL AND category_id IS NOT NULL AND thread_id IS NOT NULL)");

            migrationBuilder.AddCheckConstraint(
                name: "CK_capability_grants_scope_type_Enum",
                schema: "core_service",
                table: "capability_grants",
                sql: "scope_type BETWEEN 1 AND 4");

            migrationBuilder.AddCheckConstraint(
                name: "CK_capability_grants_source_type_Enum",
                schema: "core_service",
                table: "capability_grants",
                sql: "source_type BETWEEN 1 AND 5");

            migrationBuilder.CreateIndex(
                name: "ix_forum_sanctions_category_id",
                schema: "core_service",
                table: "forum_sanctions",
                column: "category_id");

            migrationBuilder.CreateIndex(
                name: "ix_forum_sanctions_forum_id",
                schema: "core_service",
                table: "forum_sanctions",
                column: "forum_id");

            migrationBuilder.CreateIndex(
                name: "ix_forum_sanctions_thread_id",
                schema: "core_service",
                table: "forum_sanctions",
                column: "thread_id");

            migrationBuilder.CreateIndex(
                name: "ix_forum_sanctions_user_id_scope_type_forum_id_category_id_thr",
                schema: "core_service",
                table: "forum_sanctions",
                columns: new[] { "user_id", "scope_type", "forum_id", "category_id", "thread_id", "revoked_at", "valid_until" });

            migrationBuilder.CreateIndex(
                name: "ix_forum_sanctions_user_id_type",
                schema: "core_service",
                table: "forum_sanctions",
                columns: new[] { "user_id", "type" },
                unique: true,
                filter: "scope_type = 1 AND revoked_at IS NULL");

            migrationBuilder.CreateIndex(
                name: "ix_forum_sanctions_user_id_type_category_id",
                schema: "core_service",
                table: "forum_sanctions",
                columns: new[] { "user_id", "type", "category_id" },
                unique: true,
                filter: "scope_type = 3 AND revoked_at IS NULL");

            migrationBuilder.CreateIndex(
                name: "ix_forum_sanctions_user_id_type_forum_id",
                schema: "core_service",
                table: "forum_sanctions",
                columns: new[] { "user_id", "type", "forum_id" },
                unique: true,
                filter: "scope_type = 2 AND revoked_at IS NULL");

            migrationBuilder.CreateIndex(
                name: "ix_forum_sanctions_user_id_type_thread_id",
                schema: "core_service",
                table: "forum_sanctions",
                columns: new[] { "user_id", "type", "thread_id" },
                unique: true,
                filter: "scope_type = 4 AND revoked_at IS NULL");

            migrationBuilder.AddForeignKey(
                name: "fk_capability_grants_threads_thread_id",
                schema: "core_service",
                table: "capability_grants",
                column: "thread_id",
                principalSchema: "core_service",
                principalTable: "threads",
                principalColumn: "thread_id",
                onDelete: ReferentialAction.Cascade);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "fk_capability_grants_threads_thread_id",
                schema: "core_service",
                table: "capability_grants");

            migrationBuilder.DropTable(
                name: "forum_sanctions",
                schema: "core_service");

            migrationBuilder.DropIndex(
                name: "ix_capability_grants_thread_id",
                schema: "core_service",
                table: "capability_grants");

            migrationBuilder.DropIndex(
                name: "ix_capability_grants_user_id_capability_scope_type_forum_id_ca",
                schema: "core_service",
                table: "capability_grants");

            migrationBuilder.DropIndex(
                name: "ix_capability_grants_user_id_source_type_capability",
                schema: "core_service",
                table: "capability_grants");

            migrationBuilder.DropIndex(
                name: "ix_capability_grants_user_id_source_type_capability_category_id",
                schema: "core_service",
                table: "capability_grants");

            migrationBuilder.DropIndex(
                name: "ix_capability_grants_user_id_source_type_capability_forum_id",
                schema: "core_service",
                table: "capability_grants");

            migrationBuilder.DropIndex(
                name: "ix_capability_grants_user_id_source_type_capability_thread_id",
                schema: "core_service",
                table: "capability_grants");

            migrationBuilder.DropIndex(
                name: "ix_capability_grants_user_id_source_type_forum_id_capability",
                schema: "core_service",
                table: "capability_grants");

            migrationBuilder.DropIndex(
                name: "ix_capability_grants_user_id_source_type_forum_id_revoked_at",
                schema: "core_service",
                table: "capability_grants");

            migrationBuilder.DropCheckConstraint(
                name: "CK_capability_grants_capability_Enum",
                schema: "core_service",
                table: "capability_grants");

            migrationBuilder.DropCheckConstraint(
                name: "ck_capability_grants_scope",
                schema: "core_service",
                table: "capability_grants");

            migrationBuilder.DropCheckConstraint(
                name: "CK_capability_grants_scope_type_Enum",
                schema: "core_service",
                table: "capability_grants");

            migrationBuilder.DropCheckConstraint(
                name: "CK_capability_grants_source_type_Enum",
                schema: "core_service",
                table: "capability_grants");

            migrationBuilder.DropColumn(
                name: "thread_id",
                schema: "core_service",
                table: "capability_grants");

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

            migrationBuilder.AddCheckConstraint(
                name: "CK_capability_grants_capability_Enum",
                schema: "core_service",
                table: "capability_grants",
                sql: "capability BETWEEN 1 AND 7");

            migrationBuilder.AddCheckConstraint(
                name: "ck_capability_grants_scope",
                schema: "core_service",
                table: "capability_grants",
                sql: "(scope_type = 1 AND forum_id IS NULL AND category_id IS NULL) OR (scope_type = 2 AND forum_id IS NOT NULL AND category_id IS NULL) OR (scope_type = 3 AND forum_id IS NOT NULL AND category_id IS NOT NULL)");

            migrationBuilder.AddCheckConstraint(
                name: "CK_capability_grants_scope_type_Enum",
                schema: "core_service",
                table: "capability_grants",
                sql: "scope_type BETWEEN 1 AND 3");

            migrationBuilder.AddCheckConstraint(
                name: "CK_capability_grants_source_type_Enum",
                schema: "core_service",
                table: "capability_grants",
                sql: "source_type BETWEEN 1 AND 4");
        }
    }
}
