using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace AiCoachOs.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class AddM13ClientMemory : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "AIRecommendationRecords",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    ClientId = table.Column<Guid>(type: "uuid", nullable: false),
                    CoachId = table.Column<Guid>(type: "uuid", nullable: false),
                    RecommendationCategory = table.Column<int>(type: "integer", nullable: false),
                    RecommendationText = table.Column<string>(type: "character varying(4000)", maxLength: 4000, nullable: false),
                    RationaleText = table.Column<string>(type: "character varying(4000)", maxLength: 4000, nullable: false),
                    ConfidenceStatement = table.Column<string>(type: "character varying(1000)", maxLength: 1000, nullable: false),
                    GeneratedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    AIProvider = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    AIModel = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    ReviewStatus = table.Column<int>(type: "integer", nullable: false),
                    CoachDecision = table.Column<int>(type: "integer", nullable: true),
                    CoachDecisionNote = table.Column<string>(type: "character varying(2000)", maxLength: 2000, nullable: true),
                    CoachDecisionAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    FinalImplementedPlan = table.Column<string>(type: "character varying(4000)", maxLength: 4000, nullable: true),
                    LinkedMemoryRecordId = table.Column<Guid>(type: "uuid", nullable: true),
                    KnowledgeClaimRefsJson = table.Column<string>(type: "text", nullable: false),
                    CreatedAtUtc = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    UpdatedAtUtc = table.Column<DateTime>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AIRecommendationRecords", x => x.Id);
                    table.ForeignKey(
                        name: "FK_AIRecommendationRecords_clients_ClientId",
                        column: x => x.ClientId,
                        principalTable: "clients",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "ClientAnonymizationLogs",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    ClientId = table.Column<Guid>(type: "uuid", nullable: false),
                    AnonymizedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    RequestedByCoachId = table.Column<Guid>(type: "uuid", nullable: false),
                    RecordsAnonymized = table.Column<int>(type: "integer", nullable: false),
                    AnonymizationReason = table.Column<string>(type: "character varying(1000)", maxLength: 1000, nullable: true),
                    CreatedAtUtc = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    UpdatedAtUtc = table.Column<DateTime>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ClientAnonymizationLogs", x => x.Id);
                    table.ForeignKey(
                        name: "FK_ClientAnonymizationLogs_clients_ClientId",
                        column: x => x.ClientId,
                        principalTable: "clients",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "ClientMemoryRecords",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    ClientId = table.Column<Guid>(type: "uuid", nullable: false),
                    CoachId = table.Column<Guid>(type: "uuid", nullable: false),
                    MemoryCategory = table.Column<int>(type: "integer", nullable: false),
                    RecordedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    ObservedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    SourceType = table.Column<int>(type: "integer", nullable: false),
                    SourceReference = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true),
                    SourceDescription = table.Column<string>(type: "character varying(1000)", maxLength: 1000, nullable: true),
                    ConfidenceLevel = table.Column<int>(type: "integer", nullable: false),
                    RecordStatus = table.Column<int>(type: "integer", nullable: false),
                    Content = table.Column<string>(type: "character varying(4000)", maxLength: 4000, nullable: false),
                    SupersededById = table.Column<Guid>(type: "uuid", nullable: true),
                    SupersededAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    SupersessionReason = table.Column<string>(type: "character varying(1000)", maxLength: 1000, nullable: true),
                    IsConflicted = table.Column<bool>(type: "boolean", nullable: false),
                    ConflictNotes = table.Column<string>(type: "character varying(2000)", maxLength: 2000, nullable: true),
                    CoachCorrectionNote = table.Column<string>(type: "character varying(2000)", maxLength: 2000, nullable: true),
                    CorrectedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    IsAnonymized = table.Column<bool>(type: "boolean", nullable: false),
                    AnonymizedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    CreatedAtUtc = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    UpdatedAtUtc = table.Column<DateTime>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ClientMemoryRecords", x => x.Id);
                    table.ForeignKey(
                        name: "FK_ClientMemoryRecords_ClientMemoryRecords_SupersededById",
                        column: x => x.SupersededById,
                        principalTable: "ClientMemoryRecords",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_ClientMemoryRecords_clients_ClientId",
                        column: x => x.ClientId,
                        principalTable: "clients",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "ClientMemorySnapshots",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    ClientId = table.Column<Guid>(type: "uuid", nullable: false),
                    CoachId = table.Column<Guid>(type: "uuid", nullable: false),
                    GeneratedAtUtc = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    GenerationTrigger = table.Column<int>(type: "integer", nullable: false),
                    SnapshotContentJson = table.Column<string>(type: "text", nullable: false),
                    IsStale = table.Column<bool>(type: "boolean", nullable: false),
                    IncludedRecordIdsJson = table.Column<string>(type: "text", nullable: false),
                    ExcludedConflictIdsJson = table.Column<string>(type: "text", nullable: false),
                    CreatedAtUtc = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    UpdatedAtUtc = table.Column<DateTime>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ClientMemorySnapshots", x => x.Id);
                    table.ForeignKey(
                        name: "FK_ClientMemorySnapshots_clients_ClientId",
                        column: x => x.ClientId,
                        principalTable: "clients",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "ClientMemoryConflicts",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    ClientId = table.Column<Guid>(type: "uuid", nullable: false),
                    RecordAId = table.Column<Guid>(type: "uuid", nullable: false),
                    RecordBId = table.Column<Guid>(type: "uuid", nullable: false),
                    ConflictDescription = table.Column<string>(type: "character varying(2000)", maxLength: 2000, nullable: false),
                    DetectedAtUtc = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    IsAutoDetected = table.Column<bool>(type: "boolean", nullable: false),
                    IsResolved = table.Column<bool>(type: "boolean", nullable: false),
                    ResolvedAtUtc = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    ResolvedByCoachId = table.Column<Guid>(type: "uuid", nullable: true),
                    ResolutionNote = table.Column<string>(type: "character varying(2000)", maxLength: 2000, nullable: true),
                    WinningRecordId = table.Column<Guid>(type: "uuid", nullable: true),
                    CreatedAtUtc = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    UpdatedAtUtc = table.Column<DateTime>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ClientMemoryConflicts", x => x.Id);
                    table.ForeignKey(
                        name: "FK_ClientMemoryConflicts_ClientMemoryRecords_RecordAId",
                        column: x => x.RecordAId,
                        principalTable: "ClientMemoryRecords",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_ClientMemoryConflicts_ClientMemoryRecords_RecordBId",
                        column: x => x.RecordBId,
                        principalTable: "ClientMemoryRecords",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_ClientMemoryConflicts_clients_ClientId",
                        column: x => x.ClientId,
                        principalTable: "clients",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(
                name: "IX_AIRecommendationRecords_ClientId_ReviewStatus",
                table: "AIRecommendationRecords",
                columns: new[] { "ClientId", "ReviewStatus" });

            migrationBuilder.CreateIndex(
                name: "IX_ClientAnonymizationLogs_ClientId_AnonymizedAt",
                table: "ClientAnonymizationLogs",
                columns: new[] { "ClientId", "AnonymizedAt" });

            migrationBuilder.CreateIndex(
                name: "IX_ClientMemoryConflicts_ClientId_IsResolved",
                table: "ClientMemoryConflicts",
                columns: new[] { "ClientId", "IsResolved" });

            migrationBuilder.CreateIndex(
                name: "IX_ClientMemoryConflicts_RecordAId",
                table: "ClientMemoryConflicts",
                column: "RecordAId");

            migrationBuilder.CreateIndex(
                name: "IX_ClientMemoryConflicts_RecordBId",
                table: "ClientMemoryConflicts",
                column: "RecordBId");

            migrationBuilder.CreateIndex(
                name: "IX_ClientMemoryRecords_ClientId",
                table: "ClientMemoryRecords",
                column: "ClientId");

            migrationBuilder.CreateIndex(
                name: "IX_ClientMemoryRecords_ClientId_MemoryCategory",
                table: "ClientMemoryRecords",
                columns: new[] { "ClientId", "MemoryCategory" });

            migrationBuilder.CreateIndex(
                name: "IX_ClientMemoryRecords_ClientId_RecordedAt",
                table: "ClientMemoryRecords",
                columns: new[] { "ClientId", "RecordedAt" });

            migrationBuilder.CreateIndex(
                name: "IX_ClientMemoryRecords_ClientId_RecordStatus",
                table: "ClientMemoryRecords",
                columns: new[] { "ClientId", "RecordStatus" });

            migrationBuilder.CreateIndex(
                name: "IX_ClientMemoryRecords_CoachId",
                table: "ClientMemoryRecords",
                column: "CoachId");

            migrationBuilder.CreateIndex(
                name: "IX_ClientMemoryRecords_IsAnonymized",
                table: "ClientMemoryRecords",
                column: "IsAnonymized");

            migrationBuilder.CreateIndex(
                name: "IX_ClientMemoryRecords_IsConflicted",
                table: "ClientMemoryRecords",
                column: "IsConflicted");

            migrationBuilder.CreateIndex(
                name: "IX_ClientMemoryRecords_SupersededById",
                table: "ClientMemoryRecords",
                column: "SupersededById");

            migrationBuilder.CreateIndex(
                name: "IX_ClientMemorySnapshots_ClientId_GeneratedAtUtc",
                table: "ClientMemorySnapshots",
                columns: new[] { "ClientId", "GeneratedAtUtc" });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "AIRecommendationRecords");

            migrationBuilder.DropTable(
                name: "ClientAnonymizationLogs");

            migrationBuilder.DropTable(
                name: "ClientMemoryConflicts");

            migrationBuilder.DropTable(
                name: "ClientMemorySnapshots");

            migrationBuilder.DropTable(
                name: "ClientMemoryRecords");
        }
    }
}
