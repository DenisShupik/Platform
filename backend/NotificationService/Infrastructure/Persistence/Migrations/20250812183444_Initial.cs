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
                name: "notification_service_ticker");

            migrationBuilder.EnsureSchema(
                name: "notification_service");

            migrationBuilder.CreateTable(
                name: "CronTickers",
                schema: "notification_service_ticker",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    expression = table.Column<string>(type: "text", nullable: true),
                    request = table.Column<byte[]>(type: "bytea", nullable: true),
                    retries = table.Column<int>(type: "integer", nullable: false),
                    retry_intervals = table.Column<int[]>(type: "integer[]", nullable: true),
                    function = table.Column<string>(type: "text", nullable: true),
                    description = table.Column<string>(type: "text", nullable: true),
                    init_identifier = table.Column<string>(type: "text", nullable: true),
                    created_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    updated_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_cron_tickers", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "notifiable_events",
                schema: "notification_service",
                columns: table => new
                {
                    notifiable_event_id = table.Column<Guid>(type: "uuid", nullable: false),
                    payload = table.Column<NotifiableEventPayload>(type: "jsonb", nullable: false),
                    occurred_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
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
                    channels = table.Column<byte[]>(type: "smallint[]", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_thread_subscriptions", x => new { x.user_id, x.thread_id });
                });

            migrationBuilder.CreateTable(
                name: "TimeTickers",
                schema: "notification_service_ticker",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    status = table.Column<int>(type: "integer", nullable: false),
                    lock_holder = table.Column<string>(type: "text", nullable: true),
                    request = table.Column<byte[]>(type: "bytea", nullable: true),
                    execution_time = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    locked_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    executed_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    exception = table.Column<string>(type: "text", nullable: true),
                    elapsed_time = table.Column<long>(type: "bigint", nullable: false),
                    retries = table.Column<int>(type: "integer", nullable: false),
                    retry_count = table.Column<int>(type: "integer", nullable: false),
                    retry_intervals = table.Column<int[]>(type: "integer[]", nullable: true),
                    batch_parent = table.Column<Guid>(type: "uuid", nullable: true),
                    batch_run_condition = table.Column<int>(type: "integer", nullable: true),
                    function = table.Column<string>(type: "text", nullable: true),
                    description = table.Column<string>(type: "text", nullable: true),
                    init_identifier = table.Column<string>(type: "text", nullable: true),
                    created_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    updated_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_time_tickers", x => x.id);
                    table.CheckConstraint("CK_TimeTickers_batch_run_condition_Enum", "batch_run_condition IN (0, 1)");
                    table.CheckConstraint("CK_TimeTickers_status_Enum", "status BETWEEN 0 AND 7");
                    table.ForeignKey(
                        name: "fk_time_tickers_time_tickers_batch_parent",
                        column: x => x.batch_parent,
                        principalSchema: "notification_service_ticker",
                        principalTable: "TimeTickers",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "CronTickerOccurrences",
                schema: "notification_service_ticker",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    status = table.Column<int>(type: "integer", nullable: false),
                    lock_holder = table.Column<string>(type: "text", nullable: true),
                    execution_time = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    cron_ticker_id = table.Column<Guid>(type: "uuid", nullable: false),
                    locked_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    executed_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    exception = table.Column<string>(type: "text", nullable: true),
                    elapsed_time = table.Column<long>(type: "bigint", nullable: false),
                    retry_count = table.Column<int>(type: "integer", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_cron_ticker_occurrences", x => x.id);
                    table.CheckConstraint("CK_CronTickerOccurrences_status_Enum", "status BETWEEN 0 AND 7");
                    table.ForeignKey(
                        name: "fk_cron_ticker_occurrences_cron_tickers_cron_ticker_id",
                        column: x => x.cron_ticker_id,
                        principalSchema: "notification_service_ticker",
                        principalTable: "CronTickers",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
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
                name: "IX_CronTickerOccurrence_CronTickerId",
                schema: "notification_service_ticker",
                table: "CronTickerOccurrences",
                column: "cron_ticker_id");

            migrationBuilder.CreateIndex(
                name: "IX_CronTickerOccurrence_ExecutionTime",
                schema: "notification_service_ticker",
                table: "CronTickerOccurrences",
                column: "execution_time");

            migrationBuilder.CreateIndex(
                name: "IX_CronTickerOccurrence_Status_ExecutionTime",
                schema: "notification_service_ticker",
                table: "CronTickerOccurrences",
                columns: new[] { "status", "execution_time" });

            migrationBuilder.CreateIndex(
                name: "UQ_CronTickerId_ExecutionTime",
                schema: "notification_service_ticker",
                table: "CronTickerOccurrences",
                columns: new[] { "cron_ticker_id", "execution_time" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_CronTickers_Expression",
                schema: "notification_service_ticker",
                table: "CronTickers",
                column: "expression");

            migrationBuilder.CreateIndex(
                name: "ix_notifications_notifiable_event_id",
                schema: "notification_service",
                table: "notifications",
                column: "notifiable_event_id");

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

            migrationBuilder.CreateIndex(
                name: "ix_time_tickers_batch_parent",
                schema: "notification_service_ticker",
                table: "TimeTickers",
                column: "batch_parent");

            migrationBuilder.CreateIndex(
                name: "IX_TimeTicker_ExecutionTime",
                schema: "notification_service_ticker",
                table: "TimeTickers",
                column: "execution_time");

            migrationBuilder.CreateIndex(
                name: "IX_TimeTicker_Status_ExecutionTime",
                schema: "notification_service_ticker",
                table: "TimeTickers",
                columns: new[] { "status", "execution_time" });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "CronTickerOccurrences",
                schema: "notification_service_ticker");

            migrationBuilder.DropTable(
                name: "notifications",
                schema: "notification_service");

            migrationBuilder.DropTable(
                name: "thread_subscriptions",
                schema: "notification_service");

            migrationBuilder.DropTable(
                name: "TimeTickers",
                schema: "notification_service_ticker");

            migrationBuilder.DropTable(
                name: "CronTickers",
                schema: "notification_service_ticker");

            migrationBuilder.DropTable(
                name: "notifiable_events",
                schema: "notification_service");
        }
    }
}
