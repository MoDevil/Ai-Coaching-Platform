using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace AiCoachOs.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class AddScientificKnowledgeFoundation : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "knowledge_claims",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    topic = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    question = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: false),
                    claim_text = table.Column<string>(type: "text", nullable: false),
                    evidence_level = table.Column<int>(type: "integer", nullable: false),
                    population = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true),
                    limitations = table.Column<string>(type: "text", nullable: true),
                    practical_application = table.Column<string>(type: "text", nullable: true),
                    status = table.Column<int>(type: "integer", nullable: false),
                    exercise_id = table.Column<Guid>(type: "uuid", nullable: true),
                    reviewed_at_utc = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    reviewed_by = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: true),
                    superseded_by_claim_id = table.Column<Guid>(type: "uuid", nullable: true),
                    superseded_at_utc = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    supersession_reason = table.Column<string>(type: "character varying(1000)", maxLength: 1000, nullable: true),
                    created_at_utc = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    updated_at_utc = table.Column<DateTime>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_knowledge_claims", x => x.id);
                    table.ForeignKey(
                        name: "FK_knowledge_claims_exercises_exercise_id",
                        column: x => x.exercise_id,
                        principalTable: "exercises",
                        principalColumn: "id",
                        onDelete: ReferentialAction.SetNull);
                    table.ForeignKey(
                        name: "FK_knowledge_claims_knowledge_claims_superseded_by_claim_id",
                        column: x => x.superseded_by_claim_id,
                        principalTable: "knowledge_claims",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "knowledge_sources",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    source_type = table.Column<int>(type: "integer", nullable: false),
                    title = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: false),
                    authors = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: false),
                    year = table.Column<int>(type: "integer", nullable: false),
                    doi = table.Column<string>(type: "character varying(150)", maxLength: 150, nullable: true),
                    url = table.Column<string>(type: "character varying(1000)", maxLength: 1000, nullable: true),
                    evidence_level = table.Column<int>(type: "integer", nullable: false),
                    notes = table.Column<string>(type: "text", nullable: true),
                    created_at_utc = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    updated_at_utc = table.Column<DateTime>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_knowledge_sources", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "knowledge_claim_sources",
                columns: table => new
                {
                    claim_id = table.Column<Guid>(type: "uuid", nullable: false),
                    source_id = table.Column<Guid>(type: "uuid", nullable: false),
                    relevance_note = table.Column<string>(type: "character varying(1000)", maxLength: 1000, nullable: true),
                    created_at_utc = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_knowledge_claim_sources", x => new { x.claim_id, x.source_id });
                    table.ForeignKey(
                        name: "FK_knowledge_claim_sources_knowledge_claims_claim_id",
                        column: x => x.claim_id,
                        principalTable: "knowledge_claims",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_knowledge_claim_sources_knowledge_sources_source_id",
                        column: x => x.source_id,
                        principalTable: "knowledge_sources",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_knowledge_claim_sources_source_id",
                table: "knowledge_claim_sources",
                column: "source_id");

            migrationBuilder.CreateIndex(
                name: "IX_knowledge_claims_exercise_id",
                table: "knowledge_claims",
                column: "exercise_id");

            migrationBuilder.CreateIndex(
                name: "IX_knowledge_claims_status",
                table: "knowledge_claims",
                column: "status");

            migrationBuilder.CreateIndex(
                name: "IX_knowledge_claims_superseded_by_claim_id",
                table: "knowledge_claims",
                column: "superseded_by_claim_id");

            migrationBuilder.CreateIndex(
                name: "IX_knowledge_claims_topic",
                table: "knowledge_claims",
                column: "topic");

            migrationBuilder.CreateIndex(
                name: "IX_knowledge_sources_evidence_level",
                table: "knowledge_sources",
                column: "evidence_level");

            migrationBuilder.CreateIndex(
                name: "IX_knowledge_sources_source_type",
                table: "knowledge_sources",
                column: "source_type");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "knowledge_claim_sources");

            migrationBuilder.DropTable(
                name: "knowledge_claims");

            migrationBuilder.DropTable(
                name: "knowledge_sources");
        }
    }
}
