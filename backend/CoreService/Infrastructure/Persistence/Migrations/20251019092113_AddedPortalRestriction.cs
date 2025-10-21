using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace CoreService.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class AddedPortalRestriction : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "portal_restrictions",
                schema: "core_service",
                columns: table => new
                {
                    user_id = table.Column<Guid>(type: "uuid", nullable: false),
                    type = table.Column<byte>(type: "smallint", nullable: false),
                    created_by = table.Column<Guid>(type: "uuid", nullable: false),
                    created_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    expired_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_portal_restrictions", x => new { x.user_id, x.type });
                    table.CheckConstraint("CK_portal_restrictions_type_Enum", "type BETWEEN 0 AND 4");
                });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "portal_restrictions",
                schema: "core_service");
        }
    }
}
