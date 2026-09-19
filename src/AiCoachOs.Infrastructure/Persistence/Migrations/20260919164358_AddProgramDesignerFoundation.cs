using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace AiCoachOs.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class AddProgramDesignerFoundation : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "programs",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    client_id = table.Column<Guid>(type: "uuid", nullable: false),
                    coach_id = table.Column<Guid>(type: "uuid", nullable: false),
                    name = table.Column<string>(type: "character varying(150)", maxLength: 150, nullable: false),
                    goal_primary = table.Column<int>(type: "integer", nullable: false),
                    goal_secondary = table.Column<int>(type: "integer", nullable: true),
                    goal_emphasis = table.Column<string>(type: "character varying(300)", maxLength: 300, nullable: true),
                    goal_timeline_weeks = table.Column<int>(type: "integer", nullable: true),
                    status = table.Column<int>(type: "integer", nullable: false),
                    rationale_summary = table.Column<string>(type: "character varying(2000)", maxLength: 2000, nullable: false),
                    created_at_utc = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    updated_at_utc = table.Column<DateTime>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_programs", x => x.id);
                    table.ForeignKey(
                        name: "FK_programs_clients_client_id",
                        column: x => x.client_id,
                        principalTable: "clients",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_programs_coaches_coach_id",
                        column: x => x.coach_id,
                        principalTable: "coaches",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "program_versions",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    program_id = table.Column<Guid>(type: "uuid", nullable: false),
                    version_number = table.Column<int>(type: "integer", nullable: false),
                    change_reason = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true),
                    is_active = table.Column<bool>(type: "boolean", nullable: false),
                    recovery_capacity = table.Column<int>(type: "integer", nullable: false),
                    created_at_utc = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    updated_at_utc = table.Column<DateTime>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_program_versions", x => x.id);
                    table.ForeignKey(
                        name: "FK_program_versions_programs_program_id",
                        column: x => x.program_id,
                        principalTable: "programs",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "program_muscle_priorities",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    program_version_id = table.Column<Guid>(type: "uuid", nullable: false),
                    muscle_id = table.Column<Guid>(type: "uuid", nullable: false),
                    priority_level = table.Column<int>(type: "integer", nullable: false),
                    justification = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true),
                    created_at_utc = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    updated_at_utc = table.Column<DateTime>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_program_muscle_priorities", x => x.id);
                    table.ForeignKey(
                        name: "FK_program_muscle_priorities_muscles_muscle_id",
                        column: x => x.muscle_id,
                        principalTable: "muscles",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_program_muscle_priorities_program_versions_program_version_~",
                        column: x => x.program_version_id,
                        principalTable: "program_versions",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "training_weeks",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    program_version_id = table.Column<Guid>(type: "uuid", nullable: false),
                    week_number = table.Column<int>(type: "integer", nullable: false),
                    created_at_utc = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    updated_at_utc = table.Column<DateTime>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_training_weeks", x => x.id);
                    table.ForeignKey(
                        name: "FK_training_weeks_program_versions_program_version_id",
                        column: x => x.program_version_id,
                        principalTable: "program_versions",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "training_sessions",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    training_week_id = table.Column<Guid>(type: "uuid", nullable: false),
                    day_number = table.Column<int>(type: "integer", nullable: false),
                    day_of_week = table.Column<int>(type: "integer", nullable: true),
                    name = table.Column<string>(type: "character varying(150)", maxLength: 150, nullable: false),
                    session_intent = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: false),
                    estimated_duration_minutes = table.Column<int>(type: "integer", nullable: false),
                    created_at_utc = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    updated_at_utc = table.Column<DateTime>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_training_sessions", x => x.id);
                    table.ForeignKey(
                        name: "FK_training_sessions_training_weeks_training_week_id",
                        column: x => x.training_week_id,
                        principalTable: "training_weeks",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "exercise_slots",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    training_session_id = table.Column<Guid>(type: "uuid", nullable: false),
                    exercise_id = table.Column<Guid>(type: "uuid", nullable: false),
                    order = table.Column<int>(type: "integer", nullable: false),
                    target_sets = table.Column<int>(type: "integer", nullable: false),
                    target_rep_range = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    effort_guideline = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    rest_seconds = table.Column<int>(type: "integer", nullable: false),
                    selection_rationale = table.Column<string>(type: "character varying(1000)", maxLength: 1000, nullable: false),
                    progression_type = table.Column<int>(type: "integer", nullable: true),
                    progression_target = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    progression_increment = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: true),
                    progression_condition = table.Column<string>(type: "character varying(250)", maxLength: 250, nullable: true),
                    coaching_note = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true),
                    created_at_utc = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    updated_at_utc = table.Column<DateTime>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_exercise_slots", x => x.id);
                    table.ForeignKey(
                        name: "FK_exercise_slots_exercises_exercise_id",
                        column: x => x.exercise_id,
                        principalTable: "exercises",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_exercise_slots_training_sessions_training_session_id",
                        column: x => x.training_session_id,
                        principalTable: "training_sessions",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_exercise_slots_exercise_id",
                table: "exercise_slots",
                column: "exercise_id");

            migrationBuilder.CreateIndex(
                name: "IX_exercise_slots_training_session_id_order",
                table: "exercise_slots",
                columns: new[] { "training_session_id", "order" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_program_muscle_priorities_muscle_id",
                table: "program_muscle_priorities",
                column: "muscle_id");

            migrationBuilder.CreateIndex(
                name: "IX_program_muscle_priorities_program_version_id_muscle_id",
                table: "program_muscle_priorities",
                columns: new[] { "program_version_id", "muscle_id" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_program_versions_is_active",
                table: "program_versions",
                column: "is_active");

            migrationBuilder.CreateIndex(
                name: "IX_program_versions_program_id_version_number",
                table: "program_versions",
                columns: new[] { "program_id", "version_number" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_programs_client_id",
                table: "programs",
                column: "client_id");

            migrationBuilder.CreateIndex(
                name: "IX_programs_coach_id",
                table: "programs",
                column: "coach_id");

            migrationBuilder.CreateIndex(
                name: "IX_programs_status",
                table: "programs",
                column: "status");

            migrationBuilder.CreateIndex(
                name: "IX_training_sessions_training_week_id_day_number",
                table: "training_sessions",
                columns: new[] { "training_week_id", "day_number" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_training_weeks_program_version_id_week_number",
                table: "training_weeks",
                columns: new[] { "program_version_id", "week_number" },
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "exercise_slots");

            migrationBuilder.DropTable(
                name: "program_muscle_priorities");

            migrationBuilder.DropTable(
                name: "training_sessions");

            migrationBuilder.DropTable(
                name: "training_weeks");

            migrationBuilder.DropTable(
                name: "program_versions");

            migrationBuilder.DropTable(
                name: "programs");
        }
    }
}
