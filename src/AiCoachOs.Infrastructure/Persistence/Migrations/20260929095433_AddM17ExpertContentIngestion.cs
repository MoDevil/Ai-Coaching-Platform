using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace AiCoachOs.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class AddM17ExpertContentIngestion : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "egypt_specific_notes",
                table: "knowledge_claims",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "expert_consensus",
                table: "knowledge_claims",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "expert_disagreements",
                table: "knowledge_claims",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "practitioner_notes",
                table: "knowledge_claims",
                type: "text",
                nullable: true);

            migrationBuilder.CreateTable(
                name: "expert_sources",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    name = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    channel_or_publication = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    platform = table.Column<int>(type: "integer", nullable: false),
                    primary_domain = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    credibility_tier = table.Column<int>(type: "integer", nullable: false),
                    bio = table.Column<string>(type: "character varying(2000)", maxLength: 2000, nullable: true),
                    created_at_utc = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    updated_at_utc = table.Column<DateTime>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_expert_sources", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "expert_content_ingestions",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    coach_id = table.Column<Guid>(type: "uuid", nullable: false),
                    source_id = table.Column<Guid>(type: "uuid", nullable: true),
                    source_url = table.Column<string>(type: "character varying(1000)", maxLength: 1000, nullable: false),
                    content_type = table.Column<int>(type: "integer", nullable: false),
                    title = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: false),
                    raw_extracted_text_snippet = table.Column<string>(type: "text", nullable: true),
                    word_count = table.Column<int>(type: "integer", nullable: false),
                    was_truncated = table.Column<bool>(type: "boolean", nullable: false),
                    status = table.Column<int>(type: "integer", nullable: false),
                    failure_reason = table.Column<string>(type: "character varying(1000)", maxLength: 1000, nullable: true),
                    contains_medical_claims = table.Column<bool>(type: "boolean", nullable: false),
                    medical_warning_acknowledged = table.Column<bool>(type: "boolean", nullable: false),
                    submitted_at_utc = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    completed_at_utc = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    created_at_utc = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    updated_at_utc = table.Column<DateTime>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_expert_content_ingestions", x => x.id);
                    table.ForeignKey(
                        name: "FK_expert_content_ingestions_coaches_coach_id",
                        column: x => x.coach_id,
                        principalTable: "coaches",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_expert_content_ingestions_expert_sources_source_id",
                        column: x => x.source_id,
                        principalTable: "expert_sources",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "expert_claims",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    ingestion_id = table.Column<Guid>(type: "uuid", nullable: false),
                    topic = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    sub_topic = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    claim_text = table.Column<string>(type: "text", nullable: false),
                    context_or_timestamp = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: true),
                    direct_quote = table.Column<bool>(type: "boolean", nullable: false),
                    nature_of_claim = table.Column<int>(type: "integer", nullable: false),
                    supporting_claim_id = table.Column<Guid>(type: "uuid", nullable: true),
                    conflicting_claim_id = table.Column<Guid>(type: "uuid", nullable: true),
                    review_status = table.Column<int>(type: "integer", nullable: false),
                    coach_notes = table.Column<string>(type: "character varying(1000)", maxLength: 1000, nullable: true),
                    approved_knowledge_claim_id = table.Column<Guid>(type: "uuid", nullable: true),
                    reviewed_at_utc = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    reviewed_by_coach_id = table.Column<Guid>(type: "uuid", nullable: true),
                    created_at_utc = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    updated_at_utc = table.Column<DateTime>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_expert_claims", x => x.id);
                    table.ForeignKey(
                        name: "FK_expert_claims_coaches_reviewed_by_coach_id",
                        column: x => x.reviewed_by_coach_id,
                        principalTable: "coaches",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_expert_claims_expert_content_ingestions_ingestion_id",
                        column: x => x.ingestion_id,
                        principalTable: "expert_content_ingestions",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_expert_claims_knowledge_claims_approved_knowledge_claim_id",
                        column: x => x.approved_knowledge_claim_id,
                        principalTable: "knowledge_claims",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_expert_claims_knowledge_claims_conflicting_claim_id",
                        column: x => x.conflicting_claim_id,
                        principalTable: "knowledge_claims",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_expert_claims_knowledge_claims_supporting_claim_id",
                        column: x => x.supporting_claim_id,
                        principalTable: "knowledge_claims",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(
                name: "IX_expert_claims_approved_knowledge_claim_id",
                table: "expert_claims",
                column: "approved_knowledge_claim_id");

            migrationBuilder.CreateIndex(
                name: "IX_expert_claims_conflicting_claim_id",
                table: "expert_claims",
                column: "conflicting_claim_id");

            migrationBuilder.CreateIndex(
                name: "IX_expert_claims_ingestion_id",
                table: "expert_claims",
                column: "ingestion_id");

            migrationBuilder.CreateIndex(
                name: "IX_expert_claims_review_status",
                table: "expert_claims",
                column: "review_status");

            migrationBuilder.CreateIndex(
                name: "IX_expert_claims_reviewed_by_coach_id",
                table: "expert_claims",
                column: "reviewed_by_coach_id");

            migrationBuilder.CreateIndex(
                name: "IX_expert_claims_supporting_claim_id",
                table: "expert_claims",
                column: "supporting_claim_id");

            migrationBuilder.CreateIndex(
                name: "IX_expert_claims_topic",
                table: "expert_claims",
                column: "topic");

            migrationBuilder.CreateIndex(
                name: "IX_expert_content_ingestions_coach_id",
                table: "expert_content_ingestions",
                column: "coach_id");

            migrationBuilder.CreateIndex(
                name: "IX_expert_content_ingestions_coach_id_source_url",
                table: "expert_content_ingestions",
                columns: new[] { "coach_id", "source_url" });

            migrationBuilder.CreateIndex(
                name: "IX_expert_content_ingestions_source_id",
                table: "expert_content_ingestions",
                column: "source_id");

            migrationBuilder.CreateIndex(
                name: "IX_expert_content_ingestions_status",
                table: "expert_content_ingestions",
                column: "status");

            migrationBuilder.CreateIndex(
                name: "IX_expert_content_ingestions_submitted_at_utc",
                table: "expert_content_ingestions",
                column: "submitted_at_utc");

            migrationBuilder.CreateIndex(
                name: "IX_expert_sources_credibility_tier",
                table: "expert_sources",
                column: "credibility_tier");

            migrationBuilder.CreateIndex(
                name: "IX_expert_sources_platform",
                table: "expert_sources",
                column: "platform");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "expert_claims");

            migrationBuilder.DropTable(
                name: "expert_content_ingestions");

            migrationBuilder.DropTable(
                name: "expert_sources");

            migrationBuilder.DropColumn(
                name: "egypt_specific_notes",
                table: "knowledge_claims");

            migrationBuilder.DropColumn(
                name: "expert_consensus",
                table: "knowledge_claims");

            migrationBuilder.DropColumn(
                name: "expert_disagreements",
                table: "knowledge_claims");

            migrationBuilder.DropColumn(
                name: "practitioner_notes",
                table: "knowledge_claims");
        }
    }
}
