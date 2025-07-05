using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace NotificationService.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class Initial : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.EnsureSchema(
                name: "notification_service");

            migrationBuilder.CreateTable(
                name: "thread_subscriptions",
                schema: "notification_service",
                columns: table => new
                {
                    user_id = table.Column<Guid>(type: "uuid", nullable: false),
                    thread_id = table.Column<Guid>(type: "uuid", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_thread_subscriptions", x => new { x.user_id, x.thread_id });
                });

            migrationBuilder.CreateIndex(
                name: "ix_thread_subscriptions_user_id",
                schema: "notification_service",
                table: "thread_subscriptions",
                column: "user_id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "thread_subscriptions",
                schema: "notification_service");
        }
    }
}
