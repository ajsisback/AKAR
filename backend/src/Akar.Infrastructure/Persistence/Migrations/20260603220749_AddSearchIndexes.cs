using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Akar.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class AddSearchIndexes : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateIndex(
                name: "ix_project_files_created_at_utc",
                table: "project_files",
                column: "created_at_utc");

            migrationBuilder.CreateIndex(
                name: "ix_project_files_file_category",
                table: "project_files",
                column: "file_category");

            migrationBuilder.CreateIndex(
                name: "ix_project_files_file_extension",
                table: "project_files",
                column: "file_extension");

            migrationBuilder.CreateIndex(
                name: "ix_project_files_original_file_name",
                table: "project_files",
                column: "original_file_name");

            migrationBuilder.CreateIndex(
                name: "ix_project_files_search_base",
                table: "project_files",
                columns: new[] { "project_id", "owner_id", "is_deleted", "created_at_utc" });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "ix_project_files_created_at_utc",
                table: "project_files");

            migrationBuilder.DropIndex(
                name: "ix_project_files_file_category",
                table: "project_files");

            migrationBuilder.DropIndex(
                name: "ix_project_files_file_extension",
                table: "project_files");

            migrationBuilder.DropIndex(
                name: "ix_project_files_original_file_name",
                table: "project_files");

            migrationBuilder.DropIndex(
                name: "ix_project_files_search_base",
                table: "project_files");
        }
    }
}
