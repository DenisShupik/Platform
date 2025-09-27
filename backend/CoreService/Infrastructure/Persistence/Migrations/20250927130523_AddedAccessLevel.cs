using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace CoreService.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class AddedAccessLevel : Migration
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

            migrationBuilder.AddCheckConstraint(
                name: "CK_threads_access_level_Enum",
                schema: "core_service",
                table: "threads",
                sql: "access_level IN (0, 1)");

            migrationBuilder.AddCheckConstraint(
                name: "CK_forums_access_level_Enum",
                schema: "core_service",
                table: "forums",
                sql: "access_level IN (0, 1)");

            migrationBuilder.AddCheckConstraint(
                name: "CK_categories_access_level_Enum",
                schema: "core_service",
                table: "categories",
                sql: "access_level IN (0, 1)");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
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
