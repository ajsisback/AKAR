using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Akar.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class AddProjectTimeline : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "project_timeline_events",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    project_id = table.Column<Guid>(type: "uuid", nullable: false),
                    owner_id = table.Column<Guid>(type: "uuid", nullable: false),
                    event_type = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    stage = table.Column<string>(type: "character varying(30)", maxLength: 30, nullable: false),
                    title = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    description = table.Column<string>(type: "character varying(2000)", maxLength: 2000, nullable: true),
                    event_date_utc = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    source_type = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    source_id = table.Column<Guid>(type: "uuid", nullable: true),
                    is_system_generated = table.Column<bool>(type: "boolean", nullable: false),
                    is_deleted = table.Column<bool>(type: "boolean", nullable: false, defaultValue: false),
                    deleted_at_utc = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    created_at_utc = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    updated_at_utc = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_project_timeline_events", x => x.id);
                    table.ForeignKey(
                        name: "FK_project_timeline_events_owners_owner_id",
                        column: x => x.owner_id,
                        principalTable: "owners",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_project_timeline_events_projects_project_id",
                        column: x => x.project_id,
                        principalTable: "projects",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_project_timeline_events_event_type",
                table: "project_timeline_events",
                column: "event_type");

            migrationBuilder.CreateIndex(
                name: "IX_project_timeline_events_is_deleted",
                table: "project_timeline_events",
                column: "is_deleted");

            migrationBuilder.CreateIndex(
                name: "IX_project_timeline_events_owner_id",
                table: "project_timeline_events",
                column: "owner_id");

            migrationBuilder.CreateIndex(
                name: "IX_project_timeline_events_project_id",
                table: "project_timeline_events",
                column: "project_id");

            migrationBuilder.CreateIndex(
                name: "IX_project_timeline_events_project_id_event_date_utc",
                table: "project_timeline_events",
                columns: new[] { "project_id", "event_date_utc" });

            migrationBuilder.CreateIndex(
                name: "IX_project_timeline_events_project_id_stage",
                table: "project_timeline_events",
                columns: new[] { "project_id", "stage" });

            migrationBuilder.CreateIndex(
                name: "IX_project_timeline_events_source_type",
                table: "project_timeline_events",
                column: "source_type");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "project_timeline_events");
        }
    }
}
