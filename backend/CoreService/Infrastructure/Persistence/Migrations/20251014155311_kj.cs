using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace CoreService.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class kj : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "fk_categories_policies_post_create_policy_id1",
                schema: "core_service",
                table: "categories");

            migrationBuilder.DropForeignKey(
                name: "fk_categories_policies_read_policy_id1",
                schema: "core_service",
                table: "categories");

            migrationBuilder.DropIndex(
                name: "ix_categories_post_create_policy_id1",
                schema: "core_service",
                table: "categories");

            migrationBuilder.DropIndex(
                name: "ix_categories_read_policy_id1",
                schema: "core_service",
                table: "categories");

            migrationBuilder.DropColumn(
                name: "category_thread_addable_post_create_policy_id",
                schema: "core_service",
                table: "categories");

            migrationBuilder.DropColumn(
                name: "category_thread_addable_read_policy_id",
                schema: "core_service",
                table: "categories");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<Guid>(
                name: "category_thread_addable_post_create_policy_id",
                schema: "core_service",
                table: "categories",
                type: "uuid",
                nullable: true);

            migrationBuilder.AddColumn<Guid>(
                name: "category_thread_addable_read_policy_id",
                schema: "core_service",
                table: "categories",
                type: "uuid",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "ix_categories_post_create_policy_id1",
                schema: "core_service",
                table: "categories",
                column: "category_thread_addable_post_create_policy_id");

            migrationBuilder.CreateIndex(
                name: "ix_categories_read_policy_id1",
                schema: "core_service",
                table: "categories",
                column: "category_thread_addable_read_policy_id");

            migrationBuilder.AddForeignKey(
                name: "fk_categories_policies_post_create_policy_id1",
                schema: "core_service",
                table: "categories",
                column: "category_thread_addable_post_create_policy_id",
                principalSchema: "core_service",
                principalTable: "policies",
                principalColumn: "policy_id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "fk_categories_policies_read_policy_id1",
                schema: "core_service",
                table: "categories",
                column: "category_thread_addable_read_policy_id",
                principalSchema: "core_service",
                principalTable: "policies",
                principalColumn: "policy_id",
                onDelete: ReferentialAction.Cascade);
        }
    }
}
