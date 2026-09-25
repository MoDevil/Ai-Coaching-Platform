using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace AiCoachOs.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class AddM12SubstancesAndHormoneSafety : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "PEDRedFlagRules",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    Name = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    Description = table.Column<string>(type: "character varying(1000)", maxLength: 1000, nullable: false),
                    SignalPattern = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    EscalationLevel = table.Column<int>(type: "integer", nullable: false),
                    RecommendedAction = table.Column<string>(type: "character varying(1000)", maxLength: 1000, nullable: false),
                    EvidenceBasis = table.Column<string>(type: "character varying(1000)", maxLength: 1000, nullable: false),
                    IsActive = table.Column<bool>(type: "boolean", nullable: false),
                    LastReviewedAtUtc = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    ReviewedBy = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: true),
                    KnowledgeClaimId = table.Column<Guid>(type: "uuid", nullable: true),
                    CreatedAtUtc = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    UpdatedAtUtc = table.Column<DateTime>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_PEDRedFlagRules", x => x.Id);
                    table.ForeignKey(
                        name: "FK_PEDRedFlagRules_knowledge_claims_KnowledgeClaimId",
                        column: x => x.KnowledgeClaimId,
                        principalTable: "knowledge_claims",
                        principalColumn: "id",
                        onDelete: ReferentialAction.SetNull);
                });

            migrationBuilder.CreateTable(
                name: "Substances",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    Name = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    Category = table.Column<int>(type: "integer", nullable: false),
                    Description = table.Column<string>(type: "character varying(2000)", maxLength: 2000, nullable: false),
                    EvidenceSummary = table.Column<string>(type: "character varying(4000)", maxLength: 4000, nullable: false),
                    PrimaryKnowledgeClaimId = table.Column<Guid>(type: "uuid", nullable: true),
                    LastReviewedAtUtc = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    ReviewedBy = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: true),
                    IsActive = table.Column<bool>(type: "boolean", nullable: false),
                    SafetyFlagsJson = table.Column<string>(type: "text", nullable: false),
                    HormoneAxis = table.Column<int>(type: "integer", nullable: true),
                    PhysiologicalRole = table.Column<string>(type: "character varying(2000)", maxLength: 2000, nullable: true),
                    TrainingImpactSummary = table.Column<string>(type: "character varying(2000)", maxLength: 2000, nullable: true),
                    UncertaintyStatement = table.Column<string>(type: "character varying(2000)", maxLength: 2000, nullable: true),
                    BiomarkerReferenceNotes = table.Column<string>(type: "character varying(1000)", maxLength: 1000, nullable: true),
                    PEDCategory = table.Column<int>(type: "integer", nullable: true),
                    MechanismSummary = table.Column<string>(type: "character varying(2000)", maxLength: 2000, nullable: true),
                    HealthRisksSummary = table.Column<string>(type: "character varying(4000)", maxLength: 4000, nullable: true),
                    SafetyDisclaimer = table.Column<string>(type: "character varying(1000)", maxLength: 1000, nullable: true),
                    SupplementCategory = table.Column<int>(type: "integer", nullable: true),
                    EvidenceLevel = table.Column<int>(type: "integer", nullable: true),
                    SupplementKnowledge_UncertaintyStatement = table.Column<string>(type: "character varying(2000)", maxLength: 2000, nullable: true),
                    CommonForms = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true),
                    TypicalDoseRange = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true),
                    TimingRecommendation = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true),
                    InteractionsAndNotes = table.Column<string>(type: "character varying(1000)", maxLength: 1000, nullable: true),
                    IsEgyptianMarketAvailable = table.Column<bool>(type: "boolean", nullable: true),
                    CreatedAtUtc = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    UpdatedAtUtc = table.Column<DateTime>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Substances", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Substances_knowledge_claims_PrimaryKnowledgeClaimId",
                        column: x => x.PrimaryKnowledgeClaimId,
                        principalTable: "knowledge_claims",
                        principalColumn: "id",
                        onDelete: ReferentialAction.SetNull);
                });

            migrationBuilder.CreateTable(
                name: "PEDRiskRecords",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    PEDSafetyRecordId = table.Column<Guid>(type: "uuid", nullable: false),
                    OrganSystem = table.Column<int>(type: "integer", nullable: false),
                    Severity = table.Column<int>(type: "integer", nullable: false),
                    RiskDescription = table.Column<string>(type: "character varying(2000)", maxLength: 2000, nullable: false),
                    ReversibilityNotes = table.Column<string>(type: "character varying(1000)", maxLength: 1000, nullable: true),
                    KnowledgeClaimId = table.Column<Guid>(type: "uuid", nullable: true),
                    CreatedAtUtc = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    UpdatedAtUtc = table.Column<DateTime>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_PEDRiskRecords", x => x.Id);
                    table.ForeignKey(
                        name: "FK_PEDRiskRecords_Substances_PEDSafetyRecordId",
                        column: x => x.PEDSafetyRecordId,
                        principalTable: "Substances",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_PEDRiskRecords_knowledge_claims_KnowledgeClaimId",
                        column: x => x.KnowledgeClaimId,
                        principalTable: "knowledge_claims",
                        principalColumn: "id",
                        onDelete: ReferentialAction.SetNull);
                });

            migrationBuilder.CreateTable(
                name: "SubstanceEscalationRecords",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    CoachId = table.Column<Guid>(type: "uuid", nullable: false),
                    SubstanceRecordId = table.Column<Guid>(type: "uuid", nullable: true),
                    EscalationLevel = table.Column<int>(type: "integer", nullable: false),
                    SummaryRationale = table.Column<string>(type: "character varying(4000)", maxLength: 4000, nullable: false),
                    RecommendedAction = table.Column<string>(type: "character varying(2000)", maxLength: 2000, nullable: false),
                    Disclaimer = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: false),
                    ReportedSignalsJson = table.Column<string>(type: "text", nullable: false),
                    MatchedRedFlagsJson = table.Column<string>(type: "text", nullable: false),
                    CreatedAtUtc = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    UpdatedAtUtc = table.Column<DateTime>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_SubstanceEscalationRecords", x => x.Id);
                    table.ForeignKey(
                        name: "FK_SubstanceEscalationRecords_Substances_SubstanceRecordId",
                        column: x => x.SubstanceRecordId,
                        principalTable: "Substances",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.SetNull);
                    table.ForeignKey(
                        name: "FK_SubstanceEscalationRecords_coaches_CoachId",
                        column: x => x.CoachId,
                        principalTable: "coaches",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_PEDRedFlagRules_IsActive",
                table: "PEDRedFlagRules",
                column: "IsActive");

            migrationBuilder.CreateIndex(
                name: "IX_PEDRedFlagRules_KnowledgeClaimId",
                table: "PEDRedFlagRules",
                column: "KnowledgeClaimId");

            migrationBuilder.CreateIndex(
                name: "IX_PEDRedFlagRules_SignalPattern",
                table: "PEDRedFlagRules",
                column: "SignalPattern");

            migrationBuilder.CreateIndex(
                name: "IX_PEDRiskRecords_KnowledgeClaimId",
                table: "PEDRiskRecords",
                column: "KnowledgeClaimId");

            migrationBuilder.CreateIndex(
                name: "IX_PEDRiskRecords_OrganSystem",
                table: "PEDRiskRecords",
                column: "OrganSystem");

            migrationBuilder.CreateIndex(
                name: "IX_PEDRiskRecords_PEDSafetyRecordId",
                table: "PEDRiskRecords",
                column: "PEDSafetyRecordId");

            migrationBuilder.CreateIndex(
                name: "IX_SubstanceEscalationRecords_CoachId_CreatedAtUtc",
                table: "SubstanceEscalationRecords",
                columns: new[] { "CoachId", "CreatedAtUtc" });

            migrationBuilder.CreateIndex(
                name: "IX_SubstanceEscalationRecords_SubstanceRecordId",
                table: "SubstanceEscalationRecords",
                column: "SubstanceRecordId");

            migrationBuilder.CreateIndex(
                name: "IX_Substances_Category",
                table: "Substances",
                column: "Category");

            migrationBuilder.CreateIndex(
                name: "IX_Substances_IsActive",
                table: "Substances",
                column: "IsActive");

            migrationBuilder.CreateIndex(
                name: "IX_Substances_Name",
                table: "Substances",
                column: "Name");

            migrationBuilder.CreateIndex(
                name: "IX_Substances_PrimaryKnowledgeClaimId",
                table: "Substances",
                column: "PrimaryKnowledgeClaimId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "PEDRedFlagRules");

            migrationBuilder.DropTable(
                name: "PEDRiskRecords");

            migrationBuilder.DropTable(
                name: "SubstanceEscalationRecords");

            migrationBuilder.DropTable(
                name: "Substances");
        }
    }
}
