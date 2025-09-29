using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace CoreService.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class AddedACL : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<byte>(
                name: "access_level",
                schema: "core_service",
                table: "threads",
                type: "smallint",
                nullable: false,
                defaultValue: (byte)0);

            migrationBuilder.AddColumn<byte>(
                name: "access_level",
                schema: "core_service",
                table: "forums",
                type: "smallint",
                nullable: false,
                defaultValue: (byte)0);

            migrationBuilder.AddColumn<byte>(
                name: "access_level",
                schema: "core_service",
                table: "categories",
                type: "smallint",
                nullable: false,
                defaultValue: (byte)0);

            migrationBuilder.CreateTable(
                name: "category_access_grants",
                schema: "core_service",
                columns: table => new
                {
                    user_id = table.Column<Guid>(type: "uuid", nullable: false),
                    category_id = table.Column<Guid>(type: "uuid", nullable: false),
                    created_by = table.Column<Guid>(type: "uuid", nullable: false),
                    created_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_category_access_grants", x => new { x.user_id, x.category_id });
                    table.ForeignKey(
                        name: "fk_category_access_grants_categories_category_id",
                        column: x => x.category_id,
                        principalSchema: "core_service",
                        principalTable: "categories",
                        principalColumn: "category_id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "category_access_restrictions",
                schema: "core_service",
                columns: table => new
                {
                    user_id = table.Column<Guid>(type: "uuid", nullable: false),
                    category_id = table.Column<Guid>(type: "uuid", nullable: false),
                    restriction_level = table.Column<byte>(type: "smallint", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_category_access_restrictions", x => new { x.user_id, x.category_id });
                    table.CheckConstraint("CK_category_access_restrictions_restriction_level_Enum", "restriction_level IN (0, 1)");
                    table.ForeignKey(
                        name: "fk_category_access_restrictions_categories_category_id",
                        column: x => x.category_id,
                        principalSchema: "core_service",
                        principalTable: "categories",
                        principalColumn: "category_id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "forum_access_grants",
                schema: "core_service",
                columns: table => new
                {
                    user_id = table.Column<Guid>(type: "uuid", nullable: false),
                    forum_id = table.Column<Guid>(type: "uuid", nullable: false),
                    created_by = table.Column<Guid>(type: "uuid", nullable: false),
                    created_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_forum_access_grants", x => new { x.user_id, x.forum_id });
                    table.ForeignKey(
                        name: "fk_forum_access_grants_forums_forum_id",
                        column: x => x.forum_id,
                        principalSchema: "core_service",
                        principalTable: "forums",
                        principalColumn: "forum_id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "forum_access_restrictions",
                schema: "core_service",
                columns: table => new
                {
                    user_id = table.Column<Guid>(type: "uuid", nullable: false),
                    forum_id = table.Column<Guid>(type: "uuid", nullable: false),
                    restriction_level = table.Column<byte>(type: "smallint", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_forum_access_restrictions", x => new { x.user_id, x.forum_id });
                    table.CheckConstraint("CK_forum_access_restrictions_restriction_level_Enum", "restriction_level IN (0, 1)");
                    table.ForeignKey(
                        name: "fk_forum_access_restrictions_forums_forum_id",
                        column: x => x.forum_id,
                        principalSchema: "core_service",
                        principalTable: "forums",
                        principalColumn: "forum_id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "thread_access_grants",
                schema: "core_service",
                columns: table => new
                {
                    user_id = table.Column<Guid>(type: "uuid", nullable: false),
                    thread_id = table.Column<Guid>(type: "uuid", nullable: false),
                    created_by = table.Column<Guid>(type: "uuid", nullable: false),
                    created_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_thread_access_grants", x => new { x.user_id, x.thread_id });
                    table.ForeignKey(
                        name: "fk_thread_access_grants_threads_thread_id",
                        column: x => x.thread_id,
                        principalSchema: "core_service",
                        principalTable: "threads",
                        principalColumn: "thread_id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "thread_access_restrictions",
                schema: "core_service",
                columns: table => new
                {
                    user_id = table.Column<Guid>(type: "uuid", nullable: false),
                    thread_id = table.Column<Guid>(type: "uuid", nullable: false),
                    restriction_level = table.Column<byte>(type: "smallint", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_thread_access_restrictions", x => new { x.user_id, x.thread_id });
                    table.CheckConstraint("CK_thread_access_restrictions_restriction_level_Enum", "restriction_level IN (0, 1)");
                    table.ForeignKey(
                        name: "fk_thread_access_restrictions_threads_thread_id",
                        column: x => x.thread_id,
                        principalSchema: "core_service",
                        principalTable: "threads",
                        principalColumn: "thread_id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.AddCheckConstraint(
                name: "CK_threads_access_level_Enum",
                schema: "core_service",
                table: "threads",
                sql: "access_level BETWEEN 0 AND 2");

            migrationBuilder.AddCheckConstraint(
                name: "CK_forums_access_level_Enum",
                schema: "core_service",
                table: "forums",
                sql: "access_level BETWEEN 0 AND 2");

            migrationBuilder.AddCheckConstraint(
                name: "CK_categories_access_level_Enum",
                schema: "core_service",
                table: "categories",
                sql: "access_level BETWEEN 0 AND 2");

            migrationBuilder.CreateIndex(
                name: "ix_category_access_grants_category_id",
                schema: "core_service",
                table: "category_access_grants",
                column: "category_id");

            migrationBuilder.CreateIndex(
                name: "ix_category_access_restrictions_category_id",
                schema: "core_service",
                table: "category_access_restrictions",
                column: "category_id");

            migrationBuilder.CreateIndex(
                name: "ix_forum_access_grants_forum_id",
                schema: "core_service",
                table: "forum_access_grants",
                column: "forum_id");

            migrationBuilder.CreateIndex(
                name: "ix_forum_access_restrictions_forum_id",
                schema: "core_service",
                table: "forum_access_restrictions",
                column: "forum_id");

            migrationBuilder.CreateIndex(
                name: "ix_thread_access_grants_thread_id",
                schema: "core_service",
                table: "thread_access_grants",
                column: "thread_id");

            migrationBuilder.CreateIndex(
                name: "ix_thread_access_restrictions_thread_id",
                schema: "core_service",
                table: "thread_access_restrictions",
                column: "thread_id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "category_access_grants",
                schema: "core_service");

            migrationBuilder.DropTable(
                name: "category_access_restrictions",
                schema: "core_service");

            migrationBuilder.DropTable(
                name: "forum_access_grants",
                schema: "core_service");

            migrationBuilder.DropTable(
                name: "forum_access_restrictions",
                schema: "core_service");

            migrationBuilder.DropTable(
                name: "thread_access_grants",
                schema: "core_service");

            migrationBuilder.DropTable(
                name: "thread_access_restrictions",
                schema: "core_service");

            migrationBuilder.DropCheckConstraint(
                name: "CK_threads_access_level_Enum",
                schema: "core_service",
                table: "threads");

            migrationBuilder.DropCheckConstraint(
                name: "CK_forums_access_level_Enum",
                schema: "core_service",
                table: "forums");

            migrationBuilder.DropCheckConstraint(
                name: "CK_categories_access_level_Enum",
                schema: "core_service",
                table: "categories");

            migrationBuilder.DropColumn(
                name: "access_level",
                schema: "core_service",
                table: "threads");

            migrationBuilder.DropColumn(
                name: "access_level",
                schema: "core_service",
                table: "forums");

            migrationBuilder.DropColumn(
                name: "access_level",
                schema: "core_service",
                table: "categories");
        }
    }
}
