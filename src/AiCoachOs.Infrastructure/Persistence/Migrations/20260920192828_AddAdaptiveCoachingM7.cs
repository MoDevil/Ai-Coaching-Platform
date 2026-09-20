using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace AiCoachOs.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class AddAdaptiveCoachingM7 : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "adaptation_assessments",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    program_version_id = table.Column<Guid>(type: "uuid", nullable: false),
                    assessed_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    observation_start_date = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    observation_end_date = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    total_exposures = table.Column<int>(type: "integer", nullable: false),
                    completed_exposures = table.Column<int>(type: "integer", nullable: false),
                    adherence_rate = table.Column<decimal>(type: "numeric(5,2)", precision: 5, scale: 2, nullable: false),
                    overall_status = table.Column<int>(type: "integer", nullable: false),
                    coach_notes = table.Column<string>(type: "character varying(1000)", maxLength: 1000, nullable: true),
                    created_at_utc = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    updated_at_utc = table.Column<DateTime>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_adaptation_assessments", x => x.id);
                    table.ForeignKey(
                        name: "FK_adaptation_assessments_program_versions_program_version_id",
                        column: x => x.program_version_id,
                        principalTable: "program_versions",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "exercise_adaptation_records",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    adaptation_assessment_id = table.Column<Guid>(type: "uuid", nullable: false),
                    exercise_slot_id = table.Column<Guid>(type: "uuid", nullable: false),
                    exercise_id = table.Column<Guid>(type: "uuid", nullable: false),
                    exposure_count = table.Column<int>(type: "integer", nullable: false),
                    progression_met_count = table.Column<int>(type: "integer", nullable: false),
                    effort_alignment_status = table.Column<int>(type: "integer", nullable: false),
                    performance_trend = table.Column<int>(type: "integer", nullable: false),
                    plateau_confirmed = table.Column<bool>(type: "boolean", nullable: false),
                    adherence_to_exercise = table.Column<decimal>(type: "numeric(5,2)", precision: 5, scale: 2, nullable: false),
                    created_at_utc = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    updated_at_utc = table.Column<DateTime>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_exercise_adaptation_records", x => x.id);
                    table.ForeignKey(
                        name: "FK_exercise_adaptation_records_adaptation_assessments_adaptati~",
                        column: x => x.adaptation_assessment_id,
                        principalTable: "adaptation_assessments",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_exercise_adaptation_records_exercise_slots_exercise_slot_id",
                        column: x => x.exercise_slot_id,
                        principalTable: "exercise_slots",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_exercise_adaptation_records_exercises_exercise_id",
                        column: x => x.exercise_id,
                        principalTable: "exercises",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "adaptation_recommendations",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    adaptation_assessment_id = table.Column<Guid>(type: "uuid", nullable: false),
                    exercise_adaptation_record_id = table.Column<Guid>(type: "uuid", nullable: true),
                    action_type = table.Column<int>(type: "integer", nullable: false),
                    target_slot_id = table.Column<Guid>(type: "uuid", nullable: true),
                    suggested_change_detail = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true),
                    rationale = table.Column<string>(type: "character varying(1000)", maxLength: 1000, nullable: false),
                    confidence = table.Column<int>(type: "integer", nullable: false),
                    status = table.Column<int>(type: "integer", nullable: false),
                    coach_decision_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    coach_decision_note = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true),
                    created_at_utc = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    updated_at_utc = table.Column<DateTime>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_adaptation_recommendations", x => x.id);
                    table.ForeignKey(
                        name: "FK_adaptation_recommendations_adaptation_assessments_adaptatio~",
                        column: x => x.adaptation_assessment_id,
                        principalTable: "adaptation_assessments",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_adaptation_recommendations_exercise_adaptation_records_exer~",
                        column: x => x.exercise_adaptation_record_id,
                        principalTable: "exercise_adaptation_records",
                        principalColumn: "id",
                        onDelete: ReferentialAction.SetNull);
                });

            migrationBuilder.CreateIndex(
                name: "IX_adaptation_assessments_assessed_at",
                table: "adaptation_assessments",
                column: "assessed_at");

            migrationBuilder.CreateIndex(
                name: "IX_adaptation_assessments_program_version_id",
                table: "adaptation_assessments",
                column: "program_version_id");

            migrationBuilder.CreateIndex(
                name: "IX_adaptation_recommendations_adaptation_assessment_id",
                table: "adaptation_recommendations",
                column: "adaptation_assessment_id");

            migrationBuilder.CreateIndex(
                name: "IX_adaptation_recommendations_exercise_adaptation_record_id",
                table: "adaptation_recommendations",
                column: "exercise_adaptation_record_id");

            migrationBuilder.CreateIndex(
                name: "IX_adaptation_recommendations_status",
                table: "adaptation_recommendations",
                column: "status");

            migrationBuilder.CreateIndex(
                name: "IX_exercise_adaptation_records_adaptation_assessment_id",
                table: "exercise_adaptation_records",
                column: "adaptation_assessment_id");

            migrationBuilder.CreateIndex(
                name: "IX_exercise_adaptation_records_exercise_id",
                table: "exercise_adaptation_records",
                column: "exercise_id");

            migrationBuilder.CreateIndex(
                name: "IX_exercise_adaptation_records_exercise_slot_id",
                table: "exercise_adaptation_records",
                column: "exercise_slot_id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "adaptation_recommendations");

            migrationBuilder.DropTable(
                name: "exercise_adaptation_records");

            migrationBuilder.DropTable(
                name: "adaptation_assessments");
        }
    }
}
