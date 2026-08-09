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
                name: "notifiable_events",
                schema: "notification_service",
                columns: table => new
                {
                    notifiable_event_id = table.Column<Guid>(type: "uuid", nullable: false),
                    payload = table.Column<string>(type: "jsonb", nullable: false),
                    occurred_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    thread_id = table.Column<Guid>(type: "uuid", nullable: true, computedColumnSql: "CASE WHEN \"payload\" ? 'PostId' THEN (\"payload\"->>'ThreadId')::uuid END", stored: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_notifiable_events", x => x.notifiable_event_id);
                });

            migrationBuilder.CreateTable(
                name: "thread_subscriptions",
                schema: "notification_service",
                columns: table => new
                {
                    user_id = table.Column<Guid>(type: "uuid", nullable: false),
                    thread_id = table.Column<Guid>(type: "uuid", nullable: false),
                    channels = table.Column<short[]>(type: "smallint[]", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_thread_subscriptions", x => new { x.user_id, x.thread_id });
                });

            migrationBuilder.CreateTable(
                name: "notifications",
                schema: "notification_service",
                columns: table => new
                {
                    channel = table.Column<byte>(type: "smallint", nullable: false),
                    user_id = table.Column<Guid>(type: "uuid", nullable: false),
                    notifiable_event_id = table.Column<Guid>(type: "uuid", nullable: false),
                    delivered_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_notifications", x => new { x.notifiable_event_id, x.user_id, x.channel });
                    table.CheckConstraint("CK_notifications_channel_Enum", "channel IN (0, 1)");
                    table.ForeignKey(
                        name: "fk_notifications_notifiable_events_notifiable_event_id",
                        column: x => x.notifiable_event_id,
                        principalSchema: "notification_service",
                        principalTable: "notifiable_events",
                        principalColumn: "notifiable_event_id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "ix_notifiable_events_post_thread_latest",
                schema: "notification_service",
                table: "notifiable_events",
                columns: new[] { "thread_id", "occurred_at", "notifiable_event_id" },
                descending: new[] { false, true, true });

            migrationBuilder.CreateIndex(
                name: "ix_notifications_user_id_channel_delivered_at",
                schema: "notification_service",
                table: "notifications",
                columns: new[] { "user_id", "channel", "delivered_at" })
                .Annotation("Npgsql:IndexInclude", new[] { "notifiable_event_id" });

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
                name: "notifications",
                schema: "notification_service");

            migrationBuilder.DropTable(
                name: "thread_subscriptions",
                schema: "notification_service");

            migrationBuilder.DropTable(
                name: "notifiable_events",
                schema: "notification_service");
        }
    }
}
