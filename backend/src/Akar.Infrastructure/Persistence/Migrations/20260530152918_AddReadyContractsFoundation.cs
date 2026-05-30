using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Akar.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class AddReadyContractsFoundation : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "contract_templates",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    template_code = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    template_name_ar = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    template_name_en = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    contract_type = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    description_ar = table.Column<string>(type: "character varying(2000)", maxLength: 2000, nullable: true),
                    description_en = table.Column<string>(type: "character varying(2000)", maxLength: 2000, nullable: true),
                    default_terms_json = table.Column<string>(type: "text", nullable: false),
                    required_fields_json = table.Column<string>(type: "text", nullable: false),
                    is_active = table.Column<bool>(type: "boolean", nullable: false, defaultValue: true),
                    created_at_utc = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    updated_at_utc = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_contract_templates", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "project_contracts",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    project_id = table.Column<Guid>(type: "uuid", nullable: false),
                    owner_id = table.Column<Guid>(type: "uuid", nullable: false),
                    contract_template_id = table.Column<Guid>(type: "uuid", nullable: false),
                    contract_number = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: true),
                    contract_title = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    contract_type = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    party_name = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    party_phone = table.Column<string>(type: "character varying(30)", maxLength: 30, nullable: true),
                    party_national_id = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: true),
                    contract_value = table.Column<decimal>(type: "numeric(18,2)", nullable: true),
                    start_date = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    end_date = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    contract_data_json = table.Column<string>(type: "text", nullable: false),
                    status = table.Column<string>(type: "character varying(30)", maxLength: 30, nullable: false),
                    pdf_file_id = table.Column<Guid>(type: "uuid", nullable: true),
                    signed_file_id = table.Column<Guid>(type: "uuid", nullable: true),
                    is_deleted = table.Column<bool>(type: "boolean", nullable: false, defaultValue: false),
                    deleted_at_utc = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    created_at_utc = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    updated_at_utc = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_project_contracts", x => x.id);
                    table.ForeignKey(
                        name: "FK_project_contracts_contract_templates_contract_template_id",
                        column: x => x.contract_template_id,
                        principalTable: "contract_templates",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_project_contracts_owners_owner_id",
                        column: x => x.owner_id,
                        principalTable: "owners",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_project_contracts_project_files_pdf_file_id",
                        column: x => x.pdf_file_id,
                        principalTable: "project_files",
                        principalColumn: "id",
                        onDelete: ReferentialAction.SetNull);
                    table.ForeignKey(
                        name: "FK_project_contracts_project_files_signed_file_id",
                        column: x => x.signed_file_id,
                        principalTable: "project_files",
                        principalColumn: "id",
                        onDelete: ReferentialAction.SetNull);
                    table.ForeignKey(
                        name: "FK_project_contracts_projects_project_id",
                        column: x => x.project_id,
                        principalTable: "projects",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_contract_templates_contract_type",
                table: "contract_templates",
                column: "contract_type");

            migrationBuilder.CreateIndex(
                name: "IX_contract_templates_is_active",
                table: "contract_templates",
                column: "is_active");

            migrationBuilder.CreateIndex(
                name: "IX_contract_templates_template_code",
                table: "contract_templates",
                column: "template_code",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_project_contracts_contract_template_id",
                table: "project_contracts",
                column: "contract_template_id");

            migrationBuilder.CreateIndex(
                name: "IX_project_contracts_contract_type",
                table: "project_contracts",
                column: "contract_type");

            migrationBuilder.CreateIndex(
                name: "IX_project_contracts_created_at_utc",
                table: "project_contracts",
                column: "created_at_utc");

            migrationBuilder.CreateIndex(
                name: "IX_project_contracts_is_deleted",
                table: "project_contracts",
                column: "is_deleted");

            migrationBuilder.CreateIndex(
                name: "IX_project_contracts_owner_id",
                table: "project_contracts",
                column: "owner_id");

            migrationBuilder.CreateIndex(
                name: "IX_project_contracts_pdf_file_id",
                table: "project_contracts",
                column: "pdf_file_id");

            migrationBuilder.CreateIndex(
                name: "IX_project_contracts_project_id",
                table: "project_contracts",
                column: "project_id");

            migrationBuilder.CreateIndex(
                name: "IX_project_contracts_signed_file_id",
                table: "project_contracts",
                column: "signed_file_id");

            migrationBuilder.CreateIndex(
                name: "IX_project_contracts_status",
                table: "project_contracts",
                column: "status");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "project_contracts");

            migrationBuilder.DropTable(
                name: "contract_templates");
        }
    }
}
