using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Akar.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class AddDocumentVault : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "project_folders",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    project_id = table.Column<Guid>(type: "uuid", nullable: false),
                    owner_id = table.Column<Guid>(type: "uuid", nullable: false),
                    parent_folder_id = table.Column<Guid>(type: "uuid", nullable: true),
                    folder_name = table.Column<string>(type: "character varying(300)", maxLength: 300, nullable: false),
                    folder_type = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    is_system_folder = table.Column<bool>(type: "boolean", nullable: false),
                    is_deleted = table.Column<bool>(type: "boolean", nullable: false, defaultValue: false),
                    deleted_at_utc = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    created_at_utc = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    updated_at_utc = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_project_folders", x => x.id);
                    table.ForeignKey(
                        name: "FK_project_folders_owners_owner_id",
                        column: x => x.owner_id,
                        principalTable: "owners",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_project_folders_project_folders_parent_folder_id",
                        column: x => x.parent_folder_id,
                        principalTable: "project_folders",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_project_folders_projects_project_id",
                        column: x => x.project_id,
                        principalTable: "projects",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "project_files",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    project_id = table.Column<Guid>(type: "uuid", nullable: false),
                    owner_id = table.Column<Guid>(type: "uuid", nullable: false),
                    folder_id = table.Column<Guid>(type: "uuid", nullable: false),
                    original_file_name = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: false),
                    stored_file_name = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: false),
                    content_type = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    file_extension = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    file_size_bytes = table.Column<long>(type: "bigint", nullable: false),
                    storage_provider = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    storage_path = table.Column<string>(type: "character varying(2000)", maxLength: 2000, nullable: false),
                    file_category = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    is_deleted = table.Column<bool>(type: "boolean", nullable: false, defaultValue: false),
                    deleted_at_utc = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    created_at_utc = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    updated_at_utc = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_project_files", x => x.id);
                    table.ForeignKey(
                        name: "FK_project_files_owners_owner_id",
                        column: x => x.owner_id,
                        principalTable: "owners",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_project_files_project_folders_folder_id",
                        column: x => x.folder_id,
                        principalTable: "project_folders",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_project_files_projects_project_id",
                        column: x => x.project_id,
                        principalTable: "projects",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_project_files_folder_id",
                table: "project_files",
                column: "folder_id");

            migrationBuilder.CreateIndex(
                name: "IX_project_files_is_deleted",
                table: "project_files",
                column: "is_deleted");

            migrationBuilder.CreateIndex(
                name: "IX_project_files_owner_id",
                table: "project_files",
                column: "owner_id");

            migrationBuilder.CreateIndex(
                name: "IX_project_files_project_id",
                table: "project_files",
                column: "project_id");

            migrationBuilder.CreateIndex(
                name: "IX_project_files_project_id_folder_id",
                table: "project_files",
                columns: new[] { "project_id", "folder_id" });

            migrationBuilder.CreateIndex(
                name: "IX_project_folders_owner_id",
                table: "project_folders",
                column: "owner_id");

            migrationBuilder.CreateIndex(
                name: "IX_project_folders_parent_folder_id",
                table: "project_folders",
                column: "parent_folder_id");

            migrationBuilder.CreateIndex(
                name: "IX_project_folders_project_id",
                table: "project_folders",
                column: "project_id");

            migrationBuilder.CreateIndex(
                name: "IX_project_folders_project_id_folder_type",
                table: "project_folders",
                columns: new[] { "project_id", "folder_type" });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "project_files");

            migrationBuilder.DropTable(
                name: "project_folders");
        }
    }
}
