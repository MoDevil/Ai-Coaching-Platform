using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace AiCoachOs.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class AddM8MedicalAwarenessAndSafety : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "RedFlagRules",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    Name = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    Description = table.Column<string>(type: "character varying(1000)", maxLength: 1000, nullable: false),
                    SignalPattern = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    SafetyCategoryTriggered = table.Column<int>(type: "integer", nullable: false),
                    RecommendedAction = table.Column<int>(type: "integer", nullable: false),
                    EvidenceBasis = table.Column<string>(type: "character varying(1000)", maxLength: 1000, nullable: false),
                    RequiresClinicalReview = table.Column<bool>(type: "boolean", nullable: false),
                    IsActive = table.Column<bool>(type: "boolean", nullable: false),
                    LastReviewedAtUtc = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    ReviewedBy = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: true),
                    KnowledgeClaimId = table.Column<Guid>(type: "uuid", nullable: true),
                    CreatedAtUtc = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    UpdatedAtUtc = table.Column<DateTime>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_RedFlagRules", x => x.Id);
                    table.ForeignKey(
                        name: "FK_RedFlagRules_knowledge_claims_KnowledgeClaimId",
                        column: x => x.KnowledgeClaimId,
                        principalTable: "knowledge_claims",
                        principalColumn: "id",
                        onDelete: ReferentialAction.SetNull);
                });

            migrationBuilder.CreateTable(
                name: "SafetyScreenings",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    ClientId = table.Column<Guid>(type: "uuid", nullable: false),
                    TriggeredByType = table.Column<int>(type: "integer", nullable: false),
                    TriggeredByEntityId = table.Column<Guid>(type: "uuid", nullable: true),
                    ScreeningResult = table.Column<int>(type: "integer", nullable: false),
                    RecommendedAction = table.Column<int>(type: "integer", nullable: false),
                    SummaryRationale = table.Column<string>(type: "character varying(2000)", maxLength: 2000, nullable: false),
                    Disclaimer = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: false),
                    GeneratedAtUtc = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    RequiresCoachAcknowledgment = table.Column<bool>(type: "boolean", nullable: false),
                    CoachAcknowledgedAtUtc = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    CoachNote = table.Column<string>(type: "character varying(1000)", maxLength: 1000, nullable: true),
                    ReportedSignalsJson = table.Column<string>(type: "text", nullable: false),
                    RedFlagsMatchedJson = table.Column<string>(type: "text", nullable: false),
                    CreatedAtUtc = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    UpdatedAtUtc = table.Column<DateTime>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_SafetyScreenings", x => x.Id);
                    table.ForeignKey(
                        name: "FK_SafetyScreenings_clients_ClientId",
                        column: x => x.ClientId,
                        principalTable: "clients",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_RedFlagRules_IsActive",
                table: "RedFlagRules",
                column: "IsActive");

            migrationBuilder.CreateIndex(
                name: "IX_RedFlagRules_KnowledgeClaimId",
                table: "RedFlagRules",
                column: "KnowledgeClaimId");

            migrationBuilder.CreateIndex(
                name: "IX_RedFlagRules_SignalPattern",
                table: "RedFlagRules",
                column: "SignalPattern");

            migrationBuilder.CreateIndex(
                name: "IX_SafetyScreenings_ClientId",
                table: "SafetyScreenings",
                column: "ClientId");

            migrationBuilder.CreateIndex(
                name: "IX_SafetyScreenings_GeneratedAtUtc",
                table: "SafetyScreenings",
                column: "GeneratedAtUtc");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "RedFlagRules");

            migrationBuilder.DropTable(
                name: "SafetyScreenings");
        }
    }
}
