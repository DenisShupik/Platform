using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace CoreService.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class AddedACL2 : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<byte>(
                name: "policies_post_create",
                schema: "core_service",
                table: "threads",
                type: "smallint",
                nullable: false,
                defaultValue: (byte)0);

            migrationBuilder.AddColumn<byte>(
                name: "policies_category_create",
                schema: "core_service",
                table: "forums",
                type: "smallint",
                nullable: false,
                defaultValue: (byte)0);

            migrationBuilder.AddColumn<byte>(
                name: "policies_thread_create",
                schema: "core_service",
                table: "categories",
                type: "smallint",
                nullable: false,
                defaultValue: (byte)0);

            migrationBuilder.AddCheckConstraint(
                name: "CK_threads_policies_post_create_Enum",
                schema: "core_service",
                table: "threads",
                sql: "policies_post_create IN (0, 1)");

            migrationBuilder.AddCheckConstraint(
                name: "CK_forums_policies_category_create_Enum",
                schema: "core_service",
                table: "forums",
                sql: "policies_category_create IN (0, 1)");

            migrationBuilder.AddCheckConstraint(
                name: "CK_categories_policies_thread_create_Enum",
                schema: "core_service",
                table: "categories",
                sql: "policies_thread_create IN (0, 1)");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropCheckConstraint(
                name: "CK_threads_policies_post_create_Enum",
                schema: "core_service",
                table: "threads");

            migrationBuilder.DropCheckConstraint(
                name: "CK_forums_policies_category_create_Enum",
                schema: "core_service",
                table: "forums");

            migrationBuilder.DropCheckConstraint(
                name: "CK_categories_policies_thread_create_Enum",
                schema: "core_service",
                table: "categories");

            migrationBuilder.DropColumn(
                name: "policies_post_create",
                schema: "core_service",
                table: "threads");

            migrationBuilder.DropColumn(
                name: "policies_category_create",
                schema: "core_service",
                table: "forums");

            migrationBuilder.DropColumn(
                name: "policies_thread_create",
                schema: "core_service",
                table: "categories");
        }
    }
}
