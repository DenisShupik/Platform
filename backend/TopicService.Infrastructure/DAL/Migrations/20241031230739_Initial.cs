using System;
using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace TopicService.Infrastructure.DAL.Migrations
{
    /// <inheritdoc />
    public partial class Initial : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.EnsureSchema(
                name: "topic_service");

            migrationBuilder.CreateTable(
                name: "sections",
                schema: "topic_service",
                columns: table => new
                {
                    section_id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    title = table.Column<string>(type: "character varying(256)", maxLength: 256, nullable: false),
                    created = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    created_by = table.Column<Guid>(type: "uuid", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_sections", x => x.section_id);
                });

            migrationBuilder.CreateTable(
                name: "categories",
                schema: "topic_service",
                columns: table => new
                {
                    category_id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    section_id = table.Column<long>(type: "bigint", nullable: false),
                    title = table.Column<string>(type: "character varying(256)", maxLength: 256, nullable: false),
                    created = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    created_by = table.Column<Guid>(type: "uuid", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_categories", x => x.category_id);
                    table.ForeignKey(
                        name: "fk_categories_sections_section_id",
                        column: x => x.section_id,
                        principalSchema: "topic_service",
                        principalTable: "sections",
                        principalColumn: "section_id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "topics",
                schema: "topic_service",
                columns: table => new
                {
                    topic_id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    post_id_seq = table.Column<long>(type: "bigint", nullable: false),
                    category_id = table.Column<long>(type: "bigint", nullable: false),
                    title = table.Column<string>(type: "character varying(256)", maxLength: 256, nullable: false),
                    created = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    created_by = table.Column<Guid>(type: "uuid", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_topics", x => x.topic_id);
                    table.ForeignKey(
                        name: "fk_topics_categories_category_id",
                        column: x => x.category_id,
                        principalSchema: "topic_service",
                        principalTable: "categories",
                        principalColumn: "category_id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "posts",
                schema: "topic_service",
                columns: table => new
                {
                    post_id = table.Column<long>(type: "bigint", nullable: false),
                    topic_id = table.Column<long>(type: "bigint", nullable: false),
                    content = table.Column<string>(type: "character varying(256)", maxLength: 256, nullable: false),
                    created = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    created_by = table.Column<Guid>(type: "uuid", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_posts", x => new { x.post_id, x.topic_id });
                    table.ForeignKey(
                        name: "fk_posts_topics_topic_id",
                        column: x => x.topic_id,
                        principalSchema: "topic_service",
                        principalTable: "topics",
                        principalColumn: "topic_id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "ix_categories_section_id",
                schema: "topic_service",
                table: "categories",
                column: "section_id");

            migrationBuilder.CreateIndex(
                name: "ix_posts_topic_id",
                schema: "topic_service",
                table: "posts",
                column: "topic_id");

            migrationBuilder.CreateIndex(
                name: "ix_topics_category_id",
                schema: "topic_service",
                table: "topics",
                column: "category_id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "posts",
                schema: "topic_service");

            migrationBuilder.DropTable(
                name: "topics",
                schema: "topic_service");

            migrationBuilder.DropTable(
                name: "categories",
                schema: "topic_service");

            migrationBuilder.DropTable(
                name: "sections",
                schema: "topic_service");
        }
    }
}
