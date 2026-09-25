using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace AiCoachOs.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class UpdateM12SubstanceSchema : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_PEDRedFlagRules_knowledge_claims_KnowledgeClaimId",
                table: "PEDRedFlagRules");

            migrationBuilder.DropForeignKey(
                name: "FK_PEDRiskRecords_knowledge_claims_KnowledgeClaimId",
                table: "PEDRiskRecords");

            migrationBuilder.DropColumn(
                name: "EvidenceSummary",
                table: "Substances");

            migrationBuilder.DropColumn(
                name: "HealthRisksSummary",
                table: "Substances");

            migrationBuilder.RenameColumn(
                name: "TypicalDoseRange",
                table: "Substances",
                newName: "TimingNote");

            migrationBuilder.RenameColumn(
                name: "TrainingImpactSummary",
                table: "Substances",
                newName: "TrainingRelevance");

            migrationBuilder.RenameColumn(
                name: "TimingRecommendation",
                table: "Substances",
                newName: "PrimaryClaimedBenefit");

            migrationBuilder.RenameColumn(
                name: "SupplementCategory",
                table: "Substances",
                newName: "HormoneCategory");

            migrationBuilder.RenameColumn(
                name: "HormoneAxis",
                table: "Substances",
                newName: "EvidenceStatus");

            migrationBuilder.RenameColumn(
                name: "EvidenceLevel",
                table: "Substances",
                newName: "EffectMagnitude");

            migrationBuilder.RenameColumn(
                name: "Category",
                table: "Substances",
                newName: "SubstanceCategory");

            migrationBuilder.RenameIndex(
                name: "IX_Substances_Category",
                table: "Substances",
                newName: "IX_Substances_SubstanceCategory");

            migrationBuilder.RenameColumn(
                name: "MatchedRedFlagsJson",
                table: "SubstanceEscalationRecords",
                newName: "TriggeredFlagIdsJson");

            migrationBuilder.RenameColumn(
                name: "RiskDescription",
                table: "PEDRiskRecords",
                newName: "Description");

            migrationBuilder.RenameColumn(
                name: "OrganSystem",
                table: "PEDRiskRecords",
                newName: "RiskCategory");

            migrationBuilder.RenameColumn(
                name: "KnowledgeClaimId",
                table: "PEDRiskRecords",
                newName: "EvidenceClaimId");

            migrationBuilder.RenameIndex(
                name: "IX_PEDRiskRecords_OrganSystem",
                table: "PEDRiskRecords",
                newName: "IX_PEDRiskRecords_RiskCategory");

            migrationBuilder.RenameIndex(
                name: "IX_PEDRiskRecords_KnowledgeClaimId",
                table: "PEDRiskRecords",
                newName: "IX_PEDRiskRecords_EvidenceClaimId");

            migrationBuilder.RenameColumn(
                name: "KnowledgeClaimId",
                table: "PEDRedFlagRules",
                newName: "SourceClaimId");

            migrationBuilder.RenameIndex(
                name: "IX_PEDRedFlagRules_KnowledgeClaimId",
                table: "PEDRedFlagRules",
                newName: "IX_PEDRedFlagRules_SourceClaimId");

            migrationBuilder.AddColumn<int>(
                name: "ClaimStatus",
                table: "Substances",
                type: "integer",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<string>(
                name: "CommonAliasesJson",
                table: "Substances",
                type: "text",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<Guid>(
                name: "DoseSourceClaimId",
                table: "Substances",
                type: "uuid",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "DoseUnit",
                table: "Substances",
                type: "character varying(50)",
                maxLength: 50,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "EfficacyClaim",
                table: "Substances",
                type: "character varying(2000)",
                maxLength: 2000,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "EvidenceClaimIdsJson",
                table: "Substances",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<bool>(
                name: "IsProvisional",
                table: "Substances",
                type: "boolean",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<string>(
                name: "MedicalEvaluationTriggersJson",
                table: "Substances",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "MonitoringConceptsJson",
                table: "Substances",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "PopulationNote",
                table: "Substances",
                type: "character varying(1000)",
                maxLength: 1000,
                nullable: true);

            migrationBuilder.AddColumn<bool>(
                name: "RequiresClinicalReview",
                table: "Substances",
                type: "boolean",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<DateTime>(
                name: "ReviewDueAtUtc",
                table: "Substances",
                type: "timestamp with time zone",
                nullable: true);

            migrationBuilder.AddColumn<decimal>(
                name: "TypicalDoseRangeMax",
                table: "Substances",
                type: "numeric(18,2)",
                nullable: true);

            migrationBuilder.AddColumn<decimal>(
                name: "TypicalDoseRangeMin",
                table: "Substances",
                type: "numeric(18,2)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "CoachNote",
                table: "SubstanceEscalationRecords",
                type: "character varying(2000)",
                maxLength: 2000,
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "EvidenceLevel",
                table: "PEDRiskRecords",
                type: "integer",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<int>(
                name: "PEDCategory",
                table: "PEDRedFlagRules",
                type: "integer",
                nullable: true);

            migrationBuilder.AddColumn<bool>(
                name: "RequiresClinicalReview",
                table: "PEDRedFlagRules",
                type: "boolean",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddForeignKey(
                name: "FK_PEDRedFlagRules_knowledge_claims_SourceClaimId",
                table: "PEDRedFlagRules",
                column: "SourceClaimId",
                principalTable: "knowledge_claims",
                principalColumn: "id",
                onDelete: ReferentialAction.SetNull);

            migrationBuilder.AddForeignKey(
                name: "FK_PEDRiskRecords_knowledge_claims_EvidenceClaimId",
                table: "PEDRiskRecords",
                column: "EvidenceClaimId",
                principalTable: "knowledge_claims",
                principalColumn: "id",
                onDelete: ReferentialAction.SetNull);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_PEDRedFlagRules_knowledge_claims_SourceClaimId",
                table: "PEDRedFlagRules");

            migrationBuilder.DropForeignKey(
                name: "FK_PEDRiskRecords_knowledge_claims_EvidenceClaimId",
                table: "PEDRiskRecords");

            migrationBuilder.DropColumn(
                name: "ClaimStatus",
                table: "Substances");

            migrationBuilder.DropColumn(
                name: "CommonAliasesJson",
                table: "Substances");

            migrationBuilder.DropColumn(
                name: "DoseSourceClaimId",
                table: "Substances");

            migrationBuilder.DropColumn(
                name: "DoseUnit",
                table: "Substances");

            migrationBuilder.DropColumn(
                name: "EfficacyClaim",
                table: "Substances");

            migrationBuilder.DropColumn(
                name: "EvidenceClaimIdsJson",
                table: "Substances");

            migrationBuilder.DropColumn(
                name: "IsProvisional",
                table: "Substances");

            migrationBuilder.DropColumn(
                name: "MedicalEvaluationTriggersJson",
                table: "Substances");

            migrationBuilder.DropColumn(
                name: "MonitoringConceptsJson",
                table: "Substances");

            migrationBuilder.DropColumn(
                name: "PopulationNote",
                table: "Substances");

            migrationBuilder.DropColumn(
                name: "RequiresClinicalReview",
                table: "Substances");

            migrationBuilder.DropColumn(
                name: "ReviewDueAtUtc",
                table: "Substances");

            migrationBuilder.DropColumn(
                name: "TypicalDoseRangeMax",
                table: "Substances");

            migrationBuilder.DropColumn(
                name: "TypicalDoseRangeMin",
                table: "Substances");

            migrationBuilder.DropColumn(
                name: "CoachNote",
                table: "SubstanceEscalationRecords");

            migrationBuilder.DropColumn(
                name: "EvidenceLevel",
                table: "PEDRiskRecords");

            migrationBuilder.DropColumn(
                name: "PEDCategory",
                table: "PEDRedFlagRules");

            migrationBuilder.DropColumn(
                name: "RequiresClinicalReview",
                table: "PEDRedFlagRules");

            migrationBuilder.RenameColumn(
                name: "TrainingRelevance",
                table: "Substances",
                newName: "TrainingImpactSummary");

            migrationBuilder.RenameColumn(
                name: "TimingNote",
                table: "Substances",
                newName: "TypicalDoseRange");

            migrationBuilder.RenameColumn(
                name: "SubstanceCategory",
                table: "Substances",
                newName: "Category");

            migrationBuilder.RenameColumn(
                name: "PrimaryClaimedBenefit",
                table: "Substances",
                newName: "TimingRecommendation");

            migrationBuilder.RenameColumn(
                name: "HormoneCategory",
                table: "Substances",
                newName: "SupplementCategory");

            migrationBuilder.RenameColumn(
                name: "EvidenceStatus",
                table: "Substances",
                newName: "HormoneAxis");

            migrationBuilder.RenameColumn(
                name: "EffectMagnitude",
                table: "Substances",
                newName: "EvidenceLevel");

            migrationBuilder.RenameIndex(
                name: "IX_Substances_SubstanceCategory",
                table: "Substances",
                newName: "IX_Substances_Category");

            migrationBuilder.RenameColumn(
                name: "TriggeredFlagIdsJson",
                table: "SubstanceEscalationRecords",
                newName: "MatchedRedFlagsJson");

            migrationBuilder.RenameColumn(
                name: "RiskCategory",
                table: "PEDRiskRecords",
                newName: "OrganSystem");

            migrationBuilder.RenameColumn(
                name: "EvidenceClaimId",
                table: "PEDRiskRecords",
                newName: "KnowledgeClaimId");

            migrationBuilder.RenameColumn(
                name: "Description",
                table: "PEDRiskRecords",
                newName: "RiskDescription");

            migrationBuilder.RenameIndex(
                name: "IX_PEDRiskRecords_RiskCategory",
                table: "PEDRiskRecords",
                newName: "IX_PEDRiskRecords_OrganSystem");

            migrationBuilder.RenameIndex(
                name: "IX_PEDRiskRecords_EvidenceClaimId",
                table: "PEDRiskRecords",
                newName: "IX_PEDRiskRecords_KnowledgeClaimId");

            migrationBuilder.RenameColumn(
                name: "SourceClaimId",
                table: "PEDRedFlagRules",
                newName: "KnowledgeClaimId");

            migrationBuilder.RenameIndex(
                name: "IX_PEDRedFlagRules_SourceClaimId",
                table: "PEDRedFlagRules",
                newName: "IX_PEDRedFlagRules_KnowledgeClaimId");

            migrationBuilder.AddColumn<string>(
                name: "EvidenceSummary",
                table: "Substances",
                type: "character varying(4000)",
                maxLength: 4000,
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "HealthRisksSummary",
                table: "Substances",
                type: "character varying(4000)",
                maxLength: 4000,
                nullable: true);

            migrationBuilder.AddForeignKey(
                name: "FK_PEDRedFlagRules_knowledge_claims_KnowledgeClaimId",
                table: "PEDRedFlagRules",
                column: "KnowledgeClaimId",
                principalTable: "knowledge_claims",
                principalColumn: "id",
                onDelete: ReferentialAction.SetNull);

            migrationBuilder.AddForeignKey(
                name: "FK_PEDRiskRecords_knowledge_claims_KnowledgeClaimId",
                table: "PEDRiskRecords",
                column: "KnowledgeClaimId",
                principalTable: "knowledge_claims",
                principalColumn: "id",
                onDelete: ReferentialAction.SetNull);
        }
    }
}
