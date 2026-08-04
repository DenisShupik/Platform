using Microsoft.EntityFrameworkCore.Migrations;
using NpgsqlTypes;

#nullable disable

namespace CoreService.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class FullTextSearch : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<NpgsqlTsVector>(
                name: "search_vector",
                schema: "core_service",
                table: "threads",
                type: "tsvector",
                nullable: true,
                computedColumnSql: "to_tsvector('russian', coalesce(\"title\", ''))",
                stored: true);

            migrationBuilder.AddColumn<NpgsqlTsVector>(
                name: "search_vector",
                schema: "core_service",
                table: "posts",
                type: "tsvector",
                nullable: true,
                computedColumnSql: "to_tsvector('russian', coalesce(\"content\", ''))",
                stored: true);

            migrationBuilder.AddColumn<NpgsqlTsVector>(
                name: "search_vector",
                schema: "core_service",
                table: "forums",
                type: "tsvector",
                nullable: true,
                computedColumnSql: "to_tsvector('russian', coalesce(\"title\", ''))",
                stored: true);

            migrationBuilder.AddColumn<NpgsqlTsVector>(
                name: "search_vector",
                schema: "core_service",
                table: "categories",
                type: "tsvector",
                nullable: true,
                computedColumnSql: "to_tsvector('russian', coalesce(\"title\", ''))",
                stored: true);

            migrationBuilder.CreateIndex(
                name: "ix_threads_search_vector",
                schema: "core_service",
                table: "threads",
                column: "search_vector")
                .Annotation("Npgsql:IndexMethod", "GIN");

            migrationBuilder.CreateIndex(
                name: "ix_posts_search_vector",
                schema: "core_service",
                table: "posts",
                column: "search_vector")
                .Annotation("Npgsql:IndexMethod", "GIN");

            migrationBuilder.CreateIndex(
                name: "ix_forums_search_vector",
                schema: "core_service",
                table: "forums",
                column: "search_vector")
                .Annotation("Npgsql:IndexMethod", "GIN");

            migrationBuilder.CreateIndex(
                name: "ix_categories_search_vector",
                schema: "core_service",
                table: "categories",
                column: "search_vector")
                .Annotation("Npgsql:IndexMethod", "GIN");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "ix_threads_search_vector",
                schema: "core_service",
                table: "threads");

            migrationBuilder.DropIndex(
                name: "ix_posts_search_vector",
                schema: "core_service",
                table: "posts");

            migrationBuilder.DropIndex(
                name: "ix_forums_search_vector",
                schema: "core_service",
                table: "forums");

            migrationBuilder.DropIndex(
                name: "ix_categories_search_vector",
                schema: "core_service",
                table: "categories");

            migrationBuilder.DropColumn(
                name: "search_vector",
                schema: "core_service",
                table: "threads");

            migrationBuilder.DropColumn(
                name: "search_vector",
                schema: "core_service",
                table: "posts");

            migrationBuilder.DropColumn(
                name: "search_vector",
                schema: "core_service",
                table: "forums");

            migrationBuilder.DropColumn(
                name: "search_vector",
                schema: "core_service",
                table: "categories");
        }
    }
}
