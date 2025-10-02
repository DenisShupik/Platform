using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace CoreService.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class AddedPolicies : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<Guid>(
                name: "thread_policy_set_id",
                schema: "core_service",
                table: "threads",
                type: "uuid",
                nullable: true);

            migrationBuilder.AddColumn<Guid>(
                name: "forum_policy_set_id",
                schema: "core_service",
                table: "forums",
                type: "uuid",
                nullable: false,
                defaultValue: new Guid("00000000-0000-0000-0000-000000000000"));

            migrationBuilder.AddColumn<Guid>(
                name: "category_policy_set_id",
                schema: "core_service",
                table: "categories",
                type: "uuid",
                nullable: true);

            migrationBuilder.CreateTable(
                name: "category_grants",
                schema: "core_service",
                columns: table => new
                {
                    policy = table.Column<byte>(type: "smallint", nullable: false),
                    user_id = table.Column<Guid>(type: "uuid", nullable: false),
                    category_id = table.Column<Guid>(type: "uuid", nullable: false),
                    created_by = table.Column<Guid>(type: "uuid", nullable: false),
                    created_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_category_grants", x => new { x.user_id, x.category_id, x.policy });
                    table.CheckConstraint("CK_category_grants_policy_Enum", "policy BETWEEN 0 AND 4");
                    table.ForeignKey(
                        name: "fk_category_grants_categories_category_id",
                        column: x => x.category_id,
                        principalSchema: "core_service",
                        principalTable: "categories",
                        principalColumn: "category_id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "category_policy_sets",
                schema: "core_service",
                columns: table => new
                {
                    category_policy_set_id = table.Column<Guid>(type: "uuid", nullable: false),
                    access = table.Column<byte>(type: "smallint", nullable: false),
                    updated_by = table.Column<Guid>(type: "uuid", nullable: false),
                    updated_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    post_create = table.Column<byte>(type: "smallint", nullable: false),
                    thread_create = table.Column<byte>(type: "smallint", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_category_policy_sets", x => x.category_policy_set_id);
                    table.CheckConstraint("CK_category_policy_sets_access_Enum", "access BETWEEN 0 AND 2");
                    table.CheckConstraint("CK_category_policy_sets_post_create_Enum", "post_create BETWEEN 0 AND 2");
                    table.CheckConstraint("CK_category_policy_sets_thread_create_Enum", "thread_create BETWEEN 0 AND 2");
                });

            migrationBuilder.CreateTable(
                name: "category_restrictions",
                schema: "core_service",
                columns: table => new
                {
                    policy = table.Column<byte>(type: "smallint", nullable: false),
                    user_id = table.Column<Guid>(type: "uuid", nullable: false),
                    category_id = table.Column<Guid>(type: "uuid", nullable: false),
                    created_by = table.Column<Guid>(type: "uuid", nullable: false),
                    created_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    expired_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_category_restrictions", x => new { x.user_id, x.category_id, x.policy });
                    table.CheckConstraint("CK_category_restrictions_policy_Enum", "policy BETWEEN 0 AND 4");
                    table.ForeignKey(
                        name: "fk_category_restrictions_categories_category_id",
                        column: x => x.category_id,
                        principalSchema: "core_service",
                        principalTable: "categories",
                        principalColumn: "category_id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "forum_grants",
                schema: "core_service",
                columns: table => new
                {
                    policy = table.Column<byte>(type: "smallint", nullable: false),
                    user_id = table.Column<Guid>(type: "uuid", nullable: false),
                    forum_id = table.Column<Guid>(type: "uuid", nullable: false),
                    created_by = table.Column<Guid>(type: "uuid", nullable: false),
                    created_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_forum_grants", x => new { x.user_id, x.forum_id, x.policy });
                    table.CheckConstraint("CK_forum_grants_policy_Enum", "policy BETWEEN 0 AND 4");
                    table.ForeignKey(
                        name: "fk_forum_grants_forums_forum_id",
                        column: x => x.forum_id,
                        principalSchema: "core_service",
                        principalTable: "forums",
                        principalColumn: "forum_id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "forum_policy_sets",
                schema: "core_service",
                columns: table => new
                {
                    forum_policy_set_id = table.Column<Guid>(type: "uuid", nullable: false),
                    access = table.Column<byte>(type: "smallint", nullable: false),
                    updated_by = table.Column<Guid>(type: "uuid", nullable: false),
                    updated_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    post_create = table.Column<byte>(type: "smallint", nullable: false),
                    thread_create = table.Column<byte>(type: "smallint", nullable: false),
                    category_create = table.Column<byte>(type: "smallint", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_forum_policy_sets", x => x.forum_policy_set_id);
                    table.CheckConstraint("CK_forum_policy_sets_access_Enum", "access BETWEEN 0 AND 2");
                    table.CheckConstraint("CK_forum_policy_sets_category_create_Enum", "category_create BETWEEN 0 AND 2");
                    table.CheckConstraint("CK_forum_policy_sets_post_create_Enum", "post_create BETWEEN 0 AND 2");
                    table.CheckConstraint("CK_forum_policy_sets_thread_create_Enum", "thread_create BETWEEN 0 AND 2");
                });

            migrationBuilder.CreateTable(
                name: "forum_restrictions",
                schema: "core_service",
                columns: table => new
                {
                    policy = table.Column<byte>(type: "smallint", nullable: false),
                    user_id = table.Column<Guid>(type: "uuid", nullable: false),
                    forum_id = table.Column<Guid>(type: "uuid", nullable: false),
                    created_by = table.Column<Guid>(type: "uuid", nullable: false),
                    created_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    expired_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_forum_restrictions", x => new { x.user_id, x.forum_id, x.policy });
                    table.CheckConstraint("CK_forum_restrictions_policy_Enum", "policy BETWEEN 0 AND 4");
                    table.ForeignKey(
                        name: "fk_forum_restrictions_forums_forum_id",
                        column: x => x.forum_id,
                        principalSchema: "core_service",
                        principalTable: "forums",
                        principalColumn: "forum_id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "thread_grants",
                schema: "core_service",
                columns: table => new
                {
                    policy = table.Column<byte>(type: "smallint", nullable: false),
                    user_id = table.Column<Guid>(type: "uuid", nullable: false),
                    thread_id = table.Column<Guid>(type: "uuid", nullable: false),
                    created_by = table.Column<Guid>(type: "uuid", nullable: false),
                    created_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_thread_grants", x => new { x.user_id, x.thread_id, x.policy });
                    table.CheckConstraint("CK_thread_grants_policy_Enum", "policy BETWEEN 0 AND 4");
                    table.ForeignKey(
                        name: "fk_thread_grants_threads_thread_id",
                        column: x => x.thread_id,
                        principalSchema: "core_service",
                        principalTable: "threads",
                        principalColumn: "thread_id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "thread_policy_sets",
                schema: "core_service",
                columns: table => new
                {
                    thread_policy_set_id = table.Column<Guid>(type: "uuid", nullable: false),
                    access = table.Column<byte>(type: "smallint", nullable: false),
                    updated_by = table.Column<Guid>(type: "uuid", nullable: false),
                    updated_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    post_create = table.Column<byte>(type: "smallint", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_thread_policy_sets", x => x.thread_policy_set_id);
                    table.CheckConstraint("CK_thread_policy_sets_access_Enum", "access BETWEEN 0 AND 2");
                    table.CheckConstraint("CK_thread_policy_sets_post_create_Enum", "post_create BETWEEN 0 AND 2");
                });

            migrationBuilder.CreateTable(
                name: "thread_restrictions",
                schema: "core_service",
                columns: table => new
                {
                    policy = table.Column<byte>(type: "smallint", nullable: false),
                    user_id = table.Column<Guid>(type: "uuid", nullable: false),
                    thread_id = table.Column<Guid>(type: "uuid", nullable: false),
                    created_by = table.Column<Guid>(type: "uuid", nullable: false),
                    created_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    expired_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_thread_restrictions", x => new { x.user_id, x.thread_id, x.policy });
                    table.CheckConstraint("CK_thread_restrictions_policy_Enum", "policy BETWEEN 0 AND 4");
                    table.ForeignKey(
                        name: "fk_thread_restrictions_threads_thread_id",
                        column: x => x.thread_id,
                        principalSchema: "core_service",
                        principalTable: "threads",
                        principalColumn: "thread_id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "ix_threads_thread_policy_set_id",
                schema: "core_service",
                table: "threads",
                column: "thread_policy_set_id");

            migrationBuilder.CreateIndex(
                name: "ix_forums_forum_policy_set_id",
                schema: "core_service",
                table: "forums",
                column: "forum_policy_set_id");

            migrationBuilder.CreateIndex(
                name: "ix_categories_category_policy_set_id",
                schema: "core_service",
                table: "categories",
                column: "category_policy_set_id");

            migrationBuilder.CreateIndex(
                name: "ix_category_grants_category_id",
                schema: "core_service",
                table: "category_grants",
                column: "category_id");

            migrationBuilder.CreateIndex(
                name: "ix_category_restrictions_category_id",
                schema: "core_service",
                table: "category_restrictions",
                column: "category_id");

            migrationBuilder.CreateIndex(
                name: "ix_forum_grants_forum_id",
                schema: "core_service",
                table: "forum_grants",
                column: "forum_id");

            migrationBuilder.CreateIndex(
                name: "ix_forum_restrictions_forum_id",
                schema: "core_service",
                table: "forum_restrictions",
                column: "forum_id");

            migrationBuilder.CreateIndex(
                name: "ix_thread_grants_thread_id",
                schema: "core_service",
                table: "thread_grants",
                column: "thread_id");

            migrationBuilder.CreateIndex(
                name: "ix_thread_restrictions_thread_id",
                schema: "core_service",
                table: "thread_restrictions",
                column: "thread_id");

            migrationBuilder.AddForeignKey(
                name: "fk_categories_category_policy_sets_category_policy_set_id",
                schema: "core_service",
                table: "categories",
                column: "category_policy_set_id",
                principalSchema: "core_service",
                principalTable: "category_policy_sets",
                principalColumn: "category_policy_set_id");

            migrationBuilder.AddForeignKey(
                name: "fk_forums_forum_policy_sets_forum_policy_set_id",
                schema: "core_service",
                table: "forums",
                column: "forum_policy_set_id",
                principalSchema: "core_service",
                principalTable: "forum_policy_sets",
                principalColumn: "forum_policy_set_id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "fk_threads_thread_policy_sets_thread_policy_set_id",
                schema: "core_service",
                table: "threads",
                column: "thread_policy_set_id",
                principalSchema: "core_service",
                principalTable: "thread_policy_sets",
                principalColumn: "thread_policy_set_id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "fk_categories_category_policy_sets_category_policy_set_id",
                schema: "core_service",
                table: "categories");

            migrationBuilder.DropForeignKey(
                name: "fk_forums_forum_policy_sets_forum_policy_set_id",
                schema: "core_service",
                table: "forums");

            migrationBuilder.DropForeignKey(
                name: "fk_threads_thread_policy_sets_thread_policy_set_id",
                schema: "core_service",
                table: "threads");

            migrationBuilder.DropTable(
                name: "category_grants",
                schema: "core_service");

            migrationBuilder.DropTable(
                name: "category_policy_sets",
                schema: "core_service");

            migrationBuilder.DropTable(
                name: "category_restrictions",
                schema: "core_service");

            migrationBuilder.DropTable(
                name: "forum_grants",
                schema: "core_service");

            migrationBuilder.DropTable(
                name: "forum_policy_sets",
                schema: "core_service");

            migrationBuilder.DropTable(
                name: "forum_restrictions",
                schema: "core_service");

            migrationBuilder.DropTable(
                name: "thread_grants",
                schema: "core_service");

            migrationBuilder.DropTable(
                name: "thread_policy_sets",
                schema: "core_service");

            migrationBuilder.DropTable(
                name: "thread_restrictions",
                schema: "core_service");

            migrationBuilder.DropIndex(
                name: "ix_threads_thread_policy_set_id",
                schema: "core_service",
                table: "threads");

            migrationBuilder.DropIndex(
                name: "ix_forums_forum_policy_set_id",
                schema: "core_service",
                table: "forums");

            migrationBuilder.DropIndex(
                name: "ix_categories_category_policy_set_id",
                schema: "core_service",
                table: "categories");

            migrationBuilder.DropColumn(
                name: "thread_policy_set_id",
                schema: "core_service",
                table: "threads");

            migrationBuilder.DropColumn(
                name: "forum_policy_set_id",
                schema: "core_service",
                table: "forums");

            migrationBuilder.DropColumn(
                name: "category_policy_set_id",
                schema: "core_service",
                table: "categories");
        }
    }
}
