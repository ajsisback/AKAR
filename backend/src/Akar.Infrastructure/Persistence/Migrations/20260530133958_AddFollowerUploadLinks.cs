using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Akar.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class AddFollowerUploadLinks : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "follower_upload_links",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    project_id = table.Column<Guid>(type: "uuid", nullable: false),
                    owner_id = table.Column<Guid>(type: "uuid", nullable: false),
                    follower_id = table.Column<Guid>(type: "uuid", nullable: false),
                    token_hash = table.Column<string>(type: "character varying(128)", maxLength: 128, nullable: false),
                    token_preview = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: true),
                    expires_at_utc = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    is_active = table.Column<bool>(type: "boolean", nullable: false, defaultValue: true),
                    is_revoked = table.Column<bool>(type: "boolean", nullable: false, defaultValue: false),
                    revoked_at_utc = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    last_used_at_utc = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    created_at_utc = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    updated_at_utc = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_follower_upload_links", x => x.id);
                    table.ForeignKey(
                        name: "FK_follower_upload_links_owners_owner_id",
                        column: x => x.owner_id,
                        principalTable: "owners",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_follower_upload_links_project_followers_follower_id",
                        column: x => x.follower_id,
                        principalTable: "project_followers",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_follower_upload_links_projects_project_id",
                        column: x => x.project_id,
                        principalTable: "projects",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_follower_upload_links_expires_at_utc",
                table: "follower_upload_links",
                column: "expires_at_utc");

            migrationBuilder.CreateIndex(
                name: "IX_follower_upload_links_follower_id",
                table: "follower_upload_links",
                column: "follower_id");

            migrationBuilder.CreateIndex(
                name: "IX_follower_upload_links_is_revoked",
                table: "follower_upload_links",
                column: "is_revoked");

            migrationBuilder.CreateIndex(
                name: "IX_follower_upload_links_owner_id",
                table: "follower_upload_links",
                column: "owner_id");

            migrationBuilder.CreateIndex(
                name: "IX_follower_upload_links_project_id",
                table: "follower_upload_links",
                column: "project_id");

            migrationBuilder.CreateIndex(
                name: "IX_follower_upload_links_token_hash",
                table: "follower_upload_links",
                column: "token_hash",
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "follower_upload_links");
        }
    }
}
