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
            migrationBuilder.AlterColumn<Guid>(
                name: "created_by",
                schema: "core_service",
                table: "threads",
                type: "uuid",
                nullable: true,
                oldClrType: typeof(Guid),
                oldType: "uuid");

            migrationBuilder.AddColumn<Guid>(
                name: "access_policy_id",
                schema: "core_service",
                table: "threads",
                type: "uuid",
                nullable: false,
                defaultValue: new Guid("00000000-0000-0000-0000-000000000000"));

            migrationBuilder.AddColumn<Guid>(
                name: "post_create_policy_id",
                schema: "core_service",
                table: "threads",
                type: "uuid",
                nullable: false,
                defaultValue: new Guid("00000000-0000-0000-0000-000000000000"));

            migrationBuilder.AlterColumn<Guid>(
                name: "updated_by",
                schema: "core_service",
                table: "posts",
                type: "uuid",
                nullable: true,
                oldClrType: typeof(Guid),
                oldType: "uuid");

            migrationBuilder.AlterColumn<Guid>(
                name: "created_by",
                schema: "core_service",
                table: "posts",
                type: "uuid",
                nullable: true,
                oldClrType: typeof(Guid),
                oldType: "uuid");

            migrationBuilder.AddColumn<Guid>(
                name: "access_policy_id",
                schema: "core_service",
                table: "forums",
                type: "uuid",
                nullable: false,
                defaultValue: new Guid("00000000-0000-0000-0000-000000000000"));

            migrationBuilder.AddColumn<Guid>(
                name: "category_create_policy_id",
                schema: "core_service",
                table: "forums",
                type: "uuid",
                nullable: false,
                defaultValue: new Guid("00000000-0000-0000-0000-000000000000"));

            migrationBuilder.AddColumn<Guid>(
                name: "post_create_policy_id",
                schema: "core_service",
                table: "forums",
                type: "uuid",
                nullable: false,
                defaultValue: new Guid("00000000-0000-0000-0000-000000000000"));

            migrationBuilder.AddColumn<Guid>(
                name: "thread_create_policy_id",
                schema: "core_service",
                table: "forums",
                type: "uuid",
                nullable: false,
                defaultValue: new Guid("00000000-0000-0000-0000-000000000000"));

            migrationBuilder.AlterColumn<Guid>(
                name: "created_by",
                schema: "core_service",
                table: "categories",
                type: "uuid",
                nullable: true,
                oldClrType: typeof(Guid),
                oldType: "uuid");

            migrationBuilder.AddColumn<Guid>(
                name: "access_policy_id",
                schema: "core_service",
                table: "categories",
                type: "uuid",
                nullable: false,
                defaultValue: new Guid("00000000-0000-0000-0000-000000000000"));

            migrationBuilder.AddColumn<Guid>(
                name: "post_create_policy_id",
                schema: "core_service",
                table: "categories",
                type: "uuid",
                nullable: false,
                defaultValue: new Guid("00000000-0000-0000-0000-000000000000"));

            migrationBuilder.AddColumn<Guid>(
                name: "thread_create_policy_id",
                schema: "core_service",
                table: "categories",
                type: "uuid",
                nullable: false,
                defaultValue: new Guid("00000000-0000-0000-0000-000000000000"));

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
                name: "grants",
                schema: "core_service",
                columns: table => new
                {
                    policy_id = table.Column<Guid>(type: "uuid", nullable: false),
                    user_id = table.Column<Guid>(type: "uuid", nullable: false),
                    created_by = table.Column<Guid>(type: "uuid", nullable: false),
                    created_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_grants", x => new { x.user_id, x.policy_id });
                });

            migrationBuilder.CreateTable(
                name: "policies",
                schema: "core_service",
                columns: table => new
                {
                    policy_id = table.Column<Guid>(type: "uuid", nullable: false),
                    type = table.Column<byte>(type: "smallint", nullable: false),
                    value = table.Column<byte>(type: "smallint", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_policies", x => x.policy_id);
                    table.CheckConstraint("CK_policies_type_Enum", "type BETWEEN 0 AND 4");
                    table.CheckConstraint("CK_policies_value_Enum", "value BETWEEN 0 AND 2");
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
                name: "ix_threads_access_policy_id",
                schema: "core_service",
                table: "threads",
                column: "access_policy_id");

            migrationBuilder.CreateIndex(
                name: "ix_threads_post_create_policy_id",
                schema: "core_service",
                table: "threads",
                column: "post_create_policy_id");

            migrationBuilder.CreateIndex(
                name: "ix_forums_access_policy_id",
                schema: "core_service",
                table: "forums",
                column: "access_policy_id");

            migrationBuilder.CreateIndex(
                name: "ix_forums_category_create_policy_id",
                schema: "core_service",
                table: "forums",
                column: "category_create_policy_id");

            migrationBuilder.CreateIndex(
                name: "ix_forums_post_create_policy_id",
                schema: "core_service",
                table: "forums",
                column: "post_create_policy_id");

            migrationBuilder.CreateIndex(
                name: "ix_forums_thread_create_policy_id",
                schema: "core_service",
                table: "forums",
                column: "thread_create_policy_id");

            migrationBuilder.CreateIndex(
                name: "ix_categories_access_policy_id",
                schema: "core_service",
                table: "categories",
                column: "access_policy_id");

            migrationBuilder.CreateIndex(
                name: "ix_categories_post_create_policy_id",
                schema: "core_service",
                table: "categories",
                column: "post_create_policy_id");

            migrationBuilder.CreateIndex(
                name: "ix_categories_thread_create_policy_id",
                schema: "core_service",
                table: "categories",
                column: "thread_create_policy_id");

            migrationBuilder.CreateIndex(
                name: "ix_category_restrictions_category_id",
                schema: "core_service",
                table: "category_restrictions",
                column: "category_id");

            migrationBuilder.CreateIndex(
                name: "ix_forum_restrictions_forum_id",
                schema: "core_service",
                table: "forum_restrictions",
                column: "forum_id");

            migrationBuilder.CreateIndex(
                name: "ix_thread_restrictions_thread_id",
                schema: "core_service",
                table: "thread_restrictions",
                column: "thread_id");

            migrationBuilder.AddForeignKey(
                name: "fk_categories_policies_access_policy_id",
                schema: "core_service",
                table: "categories",
                column: "access_policy_id",
                principalSchema: "core_service",
                principalTable: "policies",
                principalColumn: "policy_id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "fk_categories_policies_post_create_policy_id",
                schema: "core_service",
                table: "categories",
                column: "post_create_policy_id",
                principalSchema: "core_service",
                principalTable: "policies",
                principalColumn: "policy_id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "fk_categories_policies_thread_create_policy_id",
                schema: "core_service",
                table: "categories",
                column: "thread_create_policy_id",
                principalSchema: "core_service",
                principalTable: "policies",
                principalColumn: "policy_id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "fk_forums_policies_access_policy_id",
                schema: "core_service",
                table: "forums",
                column: "access_policy_id",
                principalSchema: "core_service",
                principalTable: "policies",
                principalColumn: "policy_id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "fk_forums_policies_category_create_policy_id",
                schema: "core_service",
                table: "forums",
                column: "category_create_policy_id",
                principalSchema: "core_service",
                principalTable: "policies",
                principalColumn: "policy_id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "fk_forums_policies_post_create_policy_id",
                schema: "core_service",
                table: "forums",
                column: "post_create_policy_id",
                principalSchema: "core_service",
                principalTable: "policies",
                principalColumn: "policy_id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "fk_forums_policies_thread_create_policy_id",
                schema: "core_service",
                table: "forums",
                column: "thread_create_policy_id",
                principalSchema: "core_service",
                principalTable: "policies",
                principalColumn: "policy_id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "fk_threads_policies_access_policy_id",
                schema: "core_service",
                table: "threads",
                column: "access_policy_id",
                principalSchema: "core_service",
                principalTable: "policies",
                principalColumn: "policy_id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "fk_threads_policies_post_create_policy_id",
                schema: "core_service",
                table: "threads",
                column: "post_create_policy_id",
                principalSchema: "core_service",
                principalTable: "policies",
                principalColumn: "policy_id",
                onDelete: ReferentialAction.Cascade);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "fk_categories_policies_access_policy_id",
                schema: "core_service",
                table: "categories");

            migrationBuilder.DropForeignKey(
                name: "fk_categories_policies_post_create_policy_id",
                schema: "core_service",
                table: "categories");

            migrationBuilder.DropForeignKey(
                name: "fk_categories_policies_thread_create_policy_id",
                schema: "core_service",
                table: "categories");

            migrationBuilder.DropForeignKey(
                name: "fk_forums_policies_access_policy_id",
                schema: "core_service",
                table: "forums");

            migrationBuilder.DropForeignKey(
                name: "fk_forums_policies_category_create_policy_id",
                schema: "core_service",
                table: "forums");

            migrationBuilder.DropForeignKey(
                name: "fk_forums_policies_post_create_policy_id",
                schema: "core_service",
                table: "forums");

            migrationBuilder.DropForeignKey(
                name: "fk_forums_policies_thread_create_policy_id",
                schema: "core_service",
                table: "forums");

            migrationBuilder.DropForeignKey(
                name: "fk_threads_policies_access_policy_id",
                schema: "core_service",
                table: "threads");

            migrationBuilder.DropForeignKey(
                name: "fk_threads_policies_post_create_policy_id",
                schema: "core_service",
                table: "threads");

            migrationBuilder.DropTable(
                name: "category_restrictions",
                schema: "core_service");

            migrationBuilder.DropTable(
                name: "forum_restrictions",
                schema: "core_service");

            migrationBuilder.DropTable(
                name: "grants",
                schema: "core_service");

            migrationBuilder.DropTable(
                name: "policies",
                schema: "core_service");

            migrationBuilder.DropTable(
                name: "thread_restrictions",
                schema: "core_service");

            migrationBuilder.DropIndex(
                name: "ix_threads_access_policy_id",
                schema: "core_service",
                table: "threads");

            migrationBuilder.DropIndex(
                name: "ix_threads_post_create_policy_id",
                schema: "core_service",
                table: "threads");

            migrationBuilder.DropIndex(
                name: "ix_forums_access_policy_id",
                schema: "core_service",
                table: "forums");

            migrationBuilder.DropIndex(
                name: "ix_forums_category_create_policy_id",
                schema: "core_service",
                table: "forums");

            migrationBuilder.DropIndex(
                name: "ix_forums_post_create_policy_id",
                schema: "core_service",
                table: "forums");

            migrationBuilder.DropIndex(
                name: "ix_forums_thread_create_policy_id",
                schema: "core_service",
                table: "forums");

            migrationBuilder.DropIndex(
                name: "ix_categories_access_policy_id",
                schema: "core_service",
                table: "categories");

            migrationBuilder.DropIndex(
                name: "ix_categories_post_create_policy_id",
                schema: "core_service",
                table: "categories");

            migrationBuilder.DropIndex(
                name: "ix_categories_thread_create_policy_id",
                schema: "core_service",
                table: "categories");

            migrationBuilder.DropColumn(
                name: "access_policy_id",
                schema: "core_service",
                table: "threads");

            migrationBuilder.DropColumn(
                name: "post_create_policy_id",
                schema: "core_service",
                table: "threads");

            migrationBuilder.DropColumn(
                name: "access_policy_id",
                schema: "core_service",
                table: "forums");

            migrationBuilder.DropColumn(
                name: "category_create_policy_id",
                schema: "core_service",
                table: "forums");

            migrationBuilder.DropColumn(
                name: "post_create_policy_id",
                schema: "core_service",
                table: "forums");

            migrationBuilder.DropColumn(
                name: "thread_create_policy_id",
                schema: "core_service",
                table: "forums");

            migrationBuilder.DropColumn(
                name: "access_policy_id",
                schema: "core_service",
                table: "categories");

            migrationBuilder.DropColumn(
                name: "post_create_policy_id",
                schema: "core_service",
                table: "categories");

            migrationBuilder.DropColumn(
                name: "thread_create_policy_id",
                schema: "core_service",
                table: "categories");

            migrationBuilder.AlterColumn<Guid>(
                name: "created_by",
                schema: "core_service",
                table: "threads",
                type: "uuid",
                nullable: false,
                defaultValue: new Guid("00000000-0000-0000-0000-000000000000"),
                oldClrType: typeof(Guid),
                oldType: "uuid",
                oldNullable: true);

            migrationBuilder.AlterColumn<Guid>(
                name: "updated_by",
                schema: "core_service",
                table: "posts",
                type: "uuid",
                nullable: false,
                defaultValue: new Guid("00000000-0000-0000-0000-000000000000"),
                oldClrType: typeof(Guid),
                oldType: "uuid",
                oldNullable: true);

            migrationBuilder.AlterColumn<Guid>(
                name: "created_by",
                schema: "core_service",
                table: "posts",
                type: "uuid",
                nullable: false,
                defaultValue: new Guid("00000000-0000-0000-0000-000000000000"),
                oldClrType: typeof(Guid),
                oldType: "uuid",
                oldNullable: true);

            migrationBuilder.AlterColumn<Guid>(
                name: "created_by",
                schema: "core_service",
                table: "categories",
                type: "uuid",
                nullable: false,
                defaultValue: new Guid("00000000-0000-0000-0000-000000000000"),
                oldClrType: typeof(Guid),
                oldType: "uuid",
                oldNullable: true);
        }
    }
}
