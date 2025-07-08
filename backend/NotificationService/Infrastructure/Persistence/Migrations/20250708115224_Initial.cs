using System;
using Microsoft.EntityFrameworkCore.Migrations;
using NotificationService.Domain.Entities;

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
                name: "notification_deliveries",
                schema: "notification_service",
                columns: table => new
                {
                    channel = table.Column<byte>(type: "smallint", nullable: false),
                    notification_id = table.Column<Guid>(type: "uuid", nullable: false),
                    user_id = table.Column<Guid>(type: "uuid", nullable: false),
                    delivered_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_notification_deliveries", x => new { x.notification_id, x.user_id, x.channel });
                    table.CheckConstraint("CK_notification_deliveries_channel_Enum", "channel IN (0, 1)");
                });

            migrationBuilder.CreateTable(
                name: "notifications",
                schema: "notification_service",
                columns: table => new
                {
                    notification_id = table.Column<Guid>(type: "uuid", nullable: false),
                    data = table.Column<NotificationData>(type: "jsonb", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_notifications", x => x.notification_id);
                });

            migrationBuilder.CreateTable(
                name: "thread_subscriptions",
                schema: "notification_service",
                columns: table => new
                {
                    user_id = table.Column<Guid>(type: "uuid", nullable: false),
                    thread_id = table.Column<Guid>(type: "uuid", nullable: false),
                    channels = table.Column<byte[]>(type: "smallint[]", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_thread_subscriptions", x => new { x.user_id, x.thread_id });
                });

            migrationBuilder.CreateIndex(
                name: "ix_notification_deliveries_notification_id",
                schema: "notification_service",
                table: "notification_deliveries",
                column: "notification_id");

            migrationBuilder.CreateIndex(
                name: "ix_thread_subscriptions_thread_id",
                schema: "notification_service",
                table: "thread_subscriptions",
                column: "thread_id");

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
                name: "notification_deliveries",
                schema: "notification_service");

            migrationBuilder.DropTable(
                name: "notifications",
                schema: "notification_service");

            migrationBuilder.DropTable(
                name: "thread_subscriptions",
                schema: "notification_service");
        }
    }
}
