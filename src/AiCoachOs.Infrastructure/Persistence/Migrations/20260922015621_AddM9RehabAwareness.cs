using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace AiCoachOs.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class AddM9RehabAwareness : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "TrainingLimitations",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    ClientId = table.Column<Guid>(type: "uuid", nullable: false),
                    SafetyScreeningId = table.Column<Guid>(type: "uuid", nullable: true),
                    AffectedBodyRegion = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    LimitationSource = table.Column<int>(type: "integer", nullable: false),
                    Status = table.Column<int>(type: "integer", nullable: false),
                    Description = table.Column<string>(type: "character varying(2000)", maxLength: 2000, nullable: true),
                    ReportedAtUtc = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    CoachActivatedM9AtUtc = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    CoachActivationNote = table.Column<string>(type: "character varying(1000)", maxLength: 1000, nullable: true),
                    ResolvedAtUtc = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    CreatedAtUtc = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    UpdatedAtUtc = table.Column<DateTime>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_TrainingLimitations", x => x.Id);
                    table.ForeignKey(
                        name: "FK_TrainingLimitations_SafetyScreenings_SafetyScreeningId",
                        column: x => x.SafetyScreeningId,
                        principalTable: "SafetyScreenings",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.SetNull);
                    table.ForeignKey(
                        name: "FK_TrainingLimitations_clients_ClientId",
                        column: x => x.ClientId,
                        principalTable: "clients",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "RehabAwarenessConsiderations",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    TrainingLimitationId = table.Column<Guid>(type: "uuid", nullable: false),
                    ExerciseId = table.Column<Guid>(type: "uuid", nullable: true),
                    ConsiderationType = table.Column<int>(type: "integer", nullable: false),
                    ConsiderationText = table.Column<string>(type: "character varying(2000)", maxLength: 2000, nullable: false),
                    KnowledgeClaimId = table.Column<Guid>(type: "uuid", nullable: true),
                    EvidenceBasis = table.Column<string>(type: "character varying(1000)", maxLength: 1000, nullable: true),
                    Disclaimer = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: false),
                    Status = table.Column<int>(type: "integer", nullable: false),
                    GeneratedAtUtc = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    CoachDecisionAtUtc = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    CoachDecisionNote = table.Column<string>(type: "character varying(1000)", maxLength: 1000, nullable: true),
                    CreatedAtUtc = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    UpdatedAtUtc = table.Column<DateTime>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_RehabAwarenessConsiderations", x => x.Id);
                    table.ForeignKey(
                        name: "FK_RehabAwarenessConsiderations_TrainingLimitations_TrainingLi~",
                        column: x => x.TrainingLimitationId,
                        principalTable: "TrainingLimitations",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_RehabAwarenessConsiderations_exercises_ExerciseId",
                        column: x => x.ExerciseId,
                        principalTable: "exercises",
                        principalColumn: "id",
                        onDelete: ReferentialAction.SetNull);
                    table.ForeignKey(
                        name: "FK_RehabAwarenessConsiderations_knowledge_claims_KnowledgeClai~",
                        column: x => x.KnowledgeClaimId,
                        principalTable: "knowledge_claims",
                        principalColumn: "id",
                        onDelete: ReferentialAction.SetNull);
                });

            migrationBuilder.CreateIndex(
                name: "IX_RehabAwarenessConsiderations_ExerciseId",
                table: "RehabAwarenessConsiderations",
                column: "ExerciseId");

            migrationBuilder.CreateIndex(
                name: "IX_RehabAwarenessConsiderations_GeneratedAtUtc",
                table: "RehabAwarenessConsiderations",
                column: "GeneratedAtUtc");

            migrationBuilder.CreateIndex(
                name: "IX_RehabAwarenessConsiderations_KnowledgeClaimId",
                table: "RehabAwarenessConsiderations",
                column: "KnowledgeClaimId");

            migrationBuilder.CreateIndex(
                name: "IX_RehabAwarenessConsiderations_Status",
                table: "RehabAwarenessConsiderations",
                column: "Status");

            migrationBuilder.CreateIndex(
                name: "IX_RehabAwarenessConsiderations_TrainingLimitationId",
                table: "RehabAwarenessConsiderations",
                column: "TrainingLimitationId");

            migrationBuilder.CreateIndex(
                name: "IX_TrainingLimitations_ClientId",
                table: "TrainingLimitations",
                column: "ClientId");

            migrationBuilder.CreateIndex(
                name: "IX_TrainingLimitations_ReportedAtUtc",
                table: "TrainingLimitations",
                column: "ReportedAtUtc");

            migrationBuilder.CreateIndex(
                name: "IX_TrainingLimitations_SafetyScreeningId",
                table: "TrainingLimitations",
                column: "SafetyScreeningId");

            migrationBuilder.CreateIndex(
                name: "IX_TrainingLimitations_Status",
                table: "TrainingLimitations",
                column: "Status");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "RehabAwarenessConsiderations");

            migrationBuilder.DropTable(
                name: "TrainingLimitations");
        }
    }
}
