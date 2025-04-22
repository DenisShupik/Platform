using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace CoreService.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class AddedThreadStatus : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "post_id_seq",
                schema: "core_service",
                table: "threads",
                newName: "next_post_id");

            migrationBuilder.AddColumn<byte>(
                name: "status",
                schema: "core_service",
                table: "threads",
                type: "smallint",
                nullable: false,
                defaultValue: (byte)0);

            migrationBuilder.AddCheckConstraint(
                name: "CK_threads_status_Enum",
                schema: "core_service",
                table: "threads",
                sql: "status IN (0, 1)");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropCheckConstraint(
                name: "CK_threads_status_Enum",
                schema: "core_service",
                table: "threads");

            migrationBuilder.DropColumn(
                name: "status",
                schema: "core_service",
                table: "threads");

            migrationBuilder.RenameColumn(
                name: "next_post_id",
                schema: "core_service",
                table: "threads",
                newName: "post_id_seq");
        }
    }
}
