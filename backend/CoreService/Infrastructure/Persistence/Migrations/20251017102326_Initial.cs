using System;
using CoreService.Domain.Enums;
using CoreService.Domain.ValueObjects;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace CoreService.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class Initial : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.EnsureSchema(
                name: "core_service");

            migrationBuilder.CreateTable(
                name: "grants",
                schema: "core_service",
                columns: table => new
                {
                    user_id = table.Column<Guid>(type: "uuid", nullable: false),
                    policy_id = table.Column<Guid>(type: "uuid", nullable: false),
                    created_by = table.Column<Guid>(type: "uuid", nullable: false),
                    created_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_grants", x => new { x.user_id, x.policy_id });
                });

            migrationBuilder.CreateTable(
                name: "policies",
                schema: "core_service",
                columns: table => new
                {
                    policy_id = table.Column<Guid>(type: "uuid", nullable: false),
                    type = table.Column<byte>(type: "smallint", nullable: false),
                    value = table.Column<byte>(type: "smallint", nullable: false),
                    parent_id = table.Column<Guid>(type: "uuid", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_policies", x => x.policy_id);
                    table.CheckConstraint("CK_policies_type_Enum", "type BETWEEN 0 AND 4");
                    table.CheckConstraint("CK_policies_value_Enum", "value BETWEEN 0 AND 2");
                    table.ForeignKey(
                        name: "fk_policies_policies_parent_id",
                        column: x => x.parent_id,
                        principalSchema: "core_service",
                        principalTable: "policies",
                        principalColumn: "policy_id");
                });

            migrationBuilder.CreateTable(
                name: "forums",
                schema: "core_service",
                columns: table => new
                {
                    forum_id = table.Column<Guid>(type: "uuid", nullable: false),
                    title = table.Column<string>(type: "character varying(64)", maxLength: 64, nullable: false),
                    created_by = table.Column<Guid>(type: "uuid", nullable: true),
                    created_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    read_policy_id = table.Column<Guid>(type: "uuid", nullable: false),
                    category_create_policy_id = table.Column<Guid>(type: "uuid", nullable: false),
                    thread_create_policy_id = table.Column<Guid>(type: "uuid", nullable: false),
                    post_create_policy_id = table.Column<Guid>(type: "uuid", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_forums", x => x.forum_id);
                    table.ForeignKey(
                        name: "fk_forums_policies_category_create_policy_id",
                        column: x => x.category_create_policy_id,
                        principalSchema: "core_service",
                        principalTable: "policies",
                        principalColumn: "policy_id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "fk_forums_policies_post_create_policy_id",
                        column: x => x.post_create_policy_id,
                        principalSchema: "core_service",
                        principalTable: "policies",
                        principalColumn: "policy_id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "fk_forums_policies_read_policy_id",
                        column: x => x.read_policy_id,
                        principalSchema: "core_service",
                        principalTable: "policies",
                        principalColumn: "policy_id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "fk_forums_policies_thread_create_policy_id",
                        column: x => x.thread_create_policy_id,
                        principalSchema: "core_service",
                        principalTable: "policies",
                        principalColumn: "policy_id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "portal",
                schema: "core_service",
                columns: table => new
                {
                    portal_id = table.Column<short>(type: "smallint", nullable: false),
                    read_policy_id = table.Column<Guid>(type: "uuid", nullable: false),
                    forum_create_policy_id = table.Column<Guid>(type: "uuid", nullable: false),
                    category_create_policy_id = table.Column<Guid>(type: "uuid", nullable: false),
                    thread_create_policy_id = table.Column<Guid>(type: "uuid", nullable: false),
                    post_create_policy_id = table.Column<Guid>(type: "uuid", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_portal", x => x.portal_id);
                    table.ForeignKey(
                        name: "fk_portal_policies_category_create_policy_id",
                        column: x => x.category_create_policy_id,
                        principalSchema: "core_service",
                        principalTable: "policies",
                        principalColumn: "policy_id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "fk_portal_policies_forum_create_policy_id",
                        column: x => x.forum_create_policy_id,
                        principalSchema: "core_service",
                        principalTable: "policies",
                        principalColumn: "policy_id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "fk_portal_policies_post_create_policy_id",
                        column: x => x.post_create_policy_id,
                        principalSchema: "core_service",
                        principalTable: "policies",
                        principalColumn: "policy_id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "fk_portal_policies_read_policy_id",
                        column: x => x.read_policy_id,
                        principalSchema: "core_service",
                        principalTable: "policies",
                        principalColumn: "policy_id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "fk_portal_policies_thread_create_policy_id",
                        column: x => x.thread_create_policy_id,
                        principalSchema: "core_service",
                        principalTable: "policies",
                        principalColumn: "policy_id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "categories",
                schema: "core_service",
                columns: table => new
                {
                    category_id = table.Column<Guid>(type: "uuid", nullable: false),
                    forum_id = table.Column<Guid>(type: "uuid", nullable: false),
                    title = table.Column<string>(type: "character varying(128)", maxLength: 128, nullable: false),
                    created_by = table.Column<Guid>(type: "uuid", nullable: true),
                    created_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    read_policy_id = table.Column<Guid>(type: "uuid", nullable: false),
                    thread_create_policy_id = table.Column<Guid>(type: "uuid", nullable: false),
                    post_create_policy_id = table.Column<Guid>(type: "uuid", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_categories", x => x.category_id);
                    table.ForeignKey(
                        name: "fk_categories_forums_forum_id",
                        column: x => x.forum_id,
                        principalSchema: "core_service",
                        principalTable: "forums",
                        principalColumn: "forum_id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "fk_categories_policies_post_create_policy_id",
                        column: x => x.post_create_policy_id,
                        principalSchema: "core_service",
                        principalTable: "policies",
                        principalColumn: "policy_id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "fk_categories_policies_read_policy_id",
                        column: x => x.read_policy_id,
                        principalSchema: "core_service",
                        principalTable: "policies",
                        principalColumn: "policy_id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "fk_categories_policies_thread_create_policy_id",
                        column: x => x.thread_create_policy_id,
                        principalSchema: "core_service",
                        principalTable: "policies",
                        principalColumn: "policy_id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "forum_restrictions",
                schema: "core_service",
                columns: table => new
                {
                    user_id = table.Column<Guid>(type: "uuid", nullable: false),
                    type = table.Column<byte>(type: "smallint", nullable: false),
                    forum_id = table.Column<Guid>(type: "uuid", nullable: false),
                    created_by = table.Column<Guid>(type: "uuid", nullable: false),
                    created_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    expired_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_forum_restrictions", x => new { x.user_id, x.forum_id, x.type });
                    table.CheckConstraint("CK_forum_restrictions_type_Enum", "type BETWEEN 0 AND 4");
                    table.ForeignKey(
                        name: "fk_forum_restrictions_forums_forum_id",
                        column: x => x.forum_id,
                        principalSchema: "core_service",
                        principalTable: "forums",
                        principalColumn: "forum_id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "category_restrictions",
                schema: "core_service",
                columns: table => new
                {
                    user_id = table.Column<Guid>(type: "uuid", nullable: false),
                    type = table.Column<byte>(type: "smallint", nullable: false),
                    category_id = table.Column<Guid>(type: "uuid", nullable: false),
                    created_by = table.Column<Guid>(type: "uuid", nullable: false),
                    created_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    expired_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_category_restrictions", x => new { x.user_id, x.category_id, x.type });
                    table.CheckConstraint("CK_category_restrictions_type_Enum", "type BETWEEN 0 AND 4");
                    table.ForeignKey(
                        name: "fk_category_restrictions_categories_category_id",
                        column: x => x.category_id,
                        principalSchema: "core_service",
                        principalTable: "categories",
                        principalColumn: "category_id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "threads",
                schema: "core_service",
                columns: table => new
                {
                    thread_id = table.Column<Guid>(type: "uuid", nullable: false),
                    category_id = table.Column<Guid>(type: "uuid", nullable: false),
                    title = table.Column<string>(type: "character varying(128)", maxLength: 128, nullable: false),
                    created_by = table.Column<Guid>(type: "uuid", nullable: true),
                    created_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    status = table.Column<byte>(type: "smallint", nullable: false),
                    read_policy_id = table.Column<Guid>(type: "uuid", nullable: false),
                    post_create_policy_id = table.Column<Guid>(type: "uuid", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_threads", x => x.thread_id);
                    table.CheckConstraint("CK_threads_status_Enum", "status IN (0, 1)");
                    table.ForeignKey(
                        name: "fk_threads_categories_category_id",
                        column: x => x.category_id,
                        principalSchema: "core_service",
                        principalTable: "categories",
                        principalColumn: "category_id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "fk_threads_policies_post_create_policy_id",
                        column: x => x.post_create_policy_id,
                        principalSchema: "core_service",
                        principalTable: "policies",
                        principalColumn: "policy_id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "fk_threads_policies_read_policy_id",
                        column: x => x.read_policy_id,
                        principalSchema: "core_service",
                        principalTable: "policies",
                        principalColumn: "policy_id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "posts",
                schema: "core_service",
                columns: table => new
                {
                    post_id = table.Column<Guid>(type: "uuid", nullable: false),
                    thread_id = table.Column<Guid>(type: "uuid", nullable: false),
                    content = table.Column<string>(type: "character varying(1024)", maxLength: 1024, nullable: false),
                    created_by = table.Column<Guid>(type: "uuid", nullable: true),
                    created_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    updated_by = table.Column<Guid>(type: "uuid", nullable: true),
                    updated_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    xmin = table.Column<uint>(type: "xid", rowVersion: true, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_posts", x => x.post_id);
                    table.ForeignKey(
                        name: "fk_posts_threads_thread_id",
                        column: x => x.thread_id,
                        principalSchema: "core_service",
                        principalTable: "threads",
                        principalColumn: "thread_id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "thread_restrictions",
                schema: "core_service",
                columns: table => new
                {
                    user_id = table.Column<Guid>(type: "uuid", nullable: false),
                    type = table.Column<byte>(type: "smallint", nullable: false),
                    thread_id = table.Column<Guid>(type: "uuid", nullable: false),
                    created_by = table.Column<Guid>(type: "uuid", nullable: false),
                    created_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    expired_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_thread_restrictions", x => new { x.user_id, x.thread_id, x.type });
                    table.CheckConstraint("CK_thread_restrictions_type_Enum", "type BETWEEN 0 AND 4");
                    table.ForeignKey(
                        name: "fk_thread_restrictions_threads_thread_id",
                        column: x => x.thread_id,
                        principalSchema: "core_service",
                        principalTable: "threads",
                        principalColumn: "thread_id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "ix_categories_forum_id",
                schema: "core_service",
                table: "categories",
                column: "forum_id");

            migrationBuilder.CreateIndex(
                name: "ix_categories_post_create_policy_id",
                schema: "core_service",
                table: "categories",
                column: "post_create_policy_id");

            migrationBuilder.CreateIndex(
                name: "ix_categories_read_policy_id",
                schema: "core_service",
                table: "categories",
                column: "read_policy_id");

            migrationBuilder.CreateIndex(
                name: "ix_categories_thread_create_policy_id",
                schema: "core_service",
                table: "categories",
                column: "thread_create_policy_id");

            migrationBuilder.CreateIndex(
                name: "ix_categories_title",
                schema: "core_service",
                table: "categories",
                column: "title");

            migrationBuilder.CreateIndex(
                name: "ix_category_restrictions_category_id",
                schema: "core_service",
                table: "category_restrictions",
                column: "category_id");

            migrationBuilder.CreateIndex(
                name: "ix_forum_restrictions_forum_id",
                schema: "core_service",
                table: "forum_restrictions",
                column: "forum_id");

            migrationBuilder.CreateIndex(
                name: "ix_forums_category_create_policy_id",
                schema: "core_service",
                table: "forums",
                column: "category_create_policy_id");

            migrationBuilder.CreateIndex(
                name: "ix_forums_post_create_policy_id",
                schema: "core_service",
                table: "forums",
                column: "post_create_policy_id");

            migrationBuilder.CreateIndex(
                name: "ix_forums_read_policy_id",
                schema: "core_service",
                table: "forums",
                column: "read_policy_id");

            migrationBuilder.CreateIndex(
                name: "ix_forums_thread_create_policy_id",
                schema: "core_service",
                table: "forums",
                column: "thread_create_policy_id");

            migrationBuilder.CreateIndex(
                name: "ix_forums_title",
                schema: "core_service",
                table: "forums",
                column: "title");

            migrationBuilder.CreateIndex(
                name: "ix_policies_parent_id",
                schema: "core_service",
                table: "policies",
                column: "parent_id");

            migrationBuilder.CreateIndex(
                name: "ix_portal_category_create_policy_id",
                schema: "core_service",
                table: "portal",
                column: "category_create_policy_id");

            migrationBuilder.CreateIndex(
                name: "ix_portal_forum_create_policy_id",
                schema: "core_service",
                table: "portal",
                column: "forum_create_policy_id");

            migrationBuilder.CreateIndex(
                name: "ix_portal_post_create_policy_id",
                schema: "core_service",
                table: "portal",
                column: "post_create_policy_id");

            migrationBuilder.CreateIndex(
                name: "ix_portal_read_policy_id",
                schema: "core_service",
                table: "portal",
                column: "read_policy_id");

            migrationBuilder.CreateIndex(
                name: "ix_portal_thread_create_policy_id",
                schema: "core_service",
                table: "portal",
                column: "thread_create_policy_id");

            migrationBuilder.CreateIndex(
                name: "ix_posts_thread_id",
                schema: "core_service",
                table: "posts",
                column: "thread_id");

            migrationBuilder.CreateIndex(
                name: "ix_posts_thread_id_created_at_post_id",
                schema: "core_service",
                table: "posts",
                columns: new[] { "thread_id", "created_at", "post_id" });

            migrationBuilder.CreateIndex(
                name: "ix_thread_restrictions_thread_id",
                schema: "core_service",
                table: "thread_restrictions",
                column: "thread_id");

            migrationBuilder.CreateIndex(
                name: "ix_threads_category_id",
                schema: "core_service",
                table: "threads",
                column: "category_id");

            migrationBuilder.CreateIndex(
                name: "ix_threads_post_create_policy_id",
                schema: "core_service",
                table: "threads",
                column: "post_create_policy_id");

            migrationBuilder.CreateIndex(
                name: "ix_threads_read_policy_id",
                schema: "core_service",
                table: "threads",
                column: "read_policy_id");
            
            var readPolicyId = PolicyId.From(Guid.CreateVersion7());
            var forumCreatePolicyId = PolicyId.From(Guid.CreateVersion7());
            var categoryCreatePolicyId = PolicyId.From(Guid.CreateVersion7());
            var threadCreatePolicyId = PolicyId.From(Guid.CreateVersion7());
            var postCreatePolicyId = PolicyId.From(Guid.CreateVersion7());
            
            migrationBuilder.InsertData(
                table: "policies",
                columns: new[] { "policy_id", "type", "value", "parent_id" },
                values: new object[,]
                {
                    { readPolicyId, (byte)PolicyType.Read, (byte)PolicyValue.Any, null },
                    { forumCreatePolicyId, (byte)PolicyType.ForumCreate, (byte)PolicyValue.Any, null },
                    { categoryCreatePolicyId, (byte)PolicyType.CategoryCreate, (byte)PolicyValue.Any, null },
                    { threadCreatePolicyId, (byte)PolicyType.ThreadCreate, (byte)PolicyValue.Any, null },
                    { postCreatePolicyId, (byte)PolicyType.PostCreate, (byte)PolicyValue.Any, null }
                });

            migrationBuilder.InsertData(
                table: "portal",
                columns: new[]
                {
                    "portal_id", "read_policy_id", "forum_create_policy_id", "category_create_policy_id",
                    "thread_create_policy_id", "post_create_policy_id"
                },
                values: new object[,]
                {
                    {
                        (short)1, readPolicyId, forumCreatePolicyId, categoryCreatePolicyId, threadCreatePolicyId,
                        postCreatePolicyId
                    }
                });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "category_restrictions",
                schema: "core_service");

            migrationBuilder.DropTable(
                name: "forum_restrictions",
                schema: "core_service");

            migrationBuilder.DropTable(
                name: "grants",
                schema: "core_service");

            migrationBuilder.DropTable(
                name: "portal",
                schema: "core_service");

            migrationBuilder.DropTable(
                name: "posts",
                schema: "core_service");

            migrationBuilder.DropTable(
                name: "thread_restrictions",
                schema: "core_service");

            migrationBuilder.DropTable(
                name: "threads",
                schema: "core_service");

            migrationBuilder.DropTable(
                name: "categories",
                schema: "core_service");

            migrationBuilder.DropTable(
                name: "forums",
                schema: "core_service");

            migrationBuilder.DropTable(
                name: "policies",
                schema: "core_service");
        }
    }
}
