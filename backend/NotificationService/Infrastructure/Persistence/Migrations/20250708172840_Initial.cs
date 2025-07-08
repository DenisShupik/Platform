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
                name: "cron_tickers",
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

            migrationBuilder.CreateTable(
                name: "time_tickers",
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
                    table.CheckConstraint("CK_time_tickers_batch_run_condition_Enum", "batch_run_condition IN (0, 1)");
                    table.CheckConstraint("CK_time_tickers_status_Enum", "status BETWEEN 0 AND 7");
                    table.ForeignKey(
                        name: "fk_time_tickers_time_tickers_batch_parent",
                        column: x => x.batch_parent,
                        principalSchema: "notification_service_ticker",
                        principalTable: "time_tickers",
                        principalColumn: "id",
                        onDelete: ReferentialAction.SetNull);
                });

            migrationBuilder.CreateTable(
                name: "cron_ticker_occurrences",
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
                    table.CheckConstraint("CK_cron_ticker_occurrences_status_Enum", "status BETWEEN 0 AND 7");
                    table.ForeignKey(
                        name: "fk_cron_ticker_occurrences_cron_tickers_cron_ticker_id",
                        column: x => x.cron_ticker_id,
                        principalSchema: "notification_service_ticker",
                        principalTable: "cron_tickers",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_CronTickerOccurrence_CronTickerId",
                schema: "notification_service_ticker",
                table: "cron_ticker_occurrences",
                column: "cron_ticker_id");

            migrationBuilder.CreateIndex(
                name: "IX_CronTickerOccurrence_ExecutionTime",
                schema: "notification_service_ticker",
                table: "cron_ticker_occurrences",
                column: "execution_time");

            migrationBuilder.CreateIndex(
                name: "IX_CronTickerOccurrence_Status_ExecutionTime",
                schema: "notification_service_ticker",
                table: "cron_ticker_occurrences",
                columns: new[] { "status", "execution_time" });

            migrationBuilder.CreateIndex(
                name: "UQ_CronTickerId_ExecutionTime",
                schema: "notification_service_ticker",
                table: "cron_ticker_occurrences",
                columns: new[] { "cron_ticker_id", "execution_time" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_CronTickers_Expression",
                schema: "notification_service_ticker",
                table: "cron_tickers",
                column: "expression");

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

            migrationBuilder.CreateIndex(
                name: "ix_time_tickers_batch_parent",
                schema: "notification_service_ticker",
                table: "time_tickers",
                column: "batch_parent");

            migrationBuilder.CreateIndex(
                name: "IX_TimeTicker_ExecutionTime",
                schema: "notification_service_ticker",
                table: "time_tickers",
                column: "execution_time");

            migrationBuilder.CreateIndex(
                name: "IX_TimeTicker_Status_ExecutionTime",
                schema: "notification_service_ticker",
                table: "time_tickers",
                columns: new[] { "status", "execution_time" });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "cron_ticker_occurrences",
                schema: "notification_service_ticker");

            migrationBuilder.DropTable(
                name: "notification_deliveries",
                schema: "notification_service");

            migrationBuilder.DropTable(
                name: "notifications",
                schema: "notification_service");

            migrationBuilder.DropTable(
                name: "thread_subscriptions",
                schema: "notification_service");

            migrationBuilder.DropTable(
                name: "time_tickers",
                schema: "notification_service_ticker");

            migrationBuilder.DropTable(
                name: "cron_tickers",
                schema: "notification_service_ticker");
        }
    }
}
