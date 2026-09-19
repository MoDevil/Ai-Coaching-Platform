using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace AiCoachOs.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class AddWorkoutLoggingAndProgression : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "workout_sessions",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    client_id = table.Column<Guid>(type: "uuid", nullable: false),
                    coach_id = table.Column<Guid>(type: "uuid", nullable: false),
                    program_version_id = table.Column<Guid>(type: "uuid", nullable: true),
                    training_session_id = table.Column<Guid>(type: "uuid", nullable: true),
                    started_at_utc = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    completed_at_utc = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    status = table.Column<int>(type: "integer", nullable: false),
                    notes = table.Column<string>(type: "character varying(1000)", maxLength: 1000, nullable: true),
                    created_at_utc = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    updated_at_utc = table.Column<DateTime>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_workout_sessions", x => x.id);
                    table.ForeignKey(
                        name: "FK_workout_sessions_clients_client_id",
                        column: x => x.client_id,
                        principalTable: "clients",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_workout_sessions_coaches_coach_id",
                        column: x => x.coach_id,
                        principalTable: "coaches",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_workout_sessions_program_versions_program_version_id",
                        column: x => x.program_version_id,
                        principalTable: "program_versions",
                        principalColumn: "id",
                        onDelete: ReferentialAction.SetNull);
                    table.ForeignKey(
                        name: "FK_workout_sessions_training_sessions_training_session_id",
                        column: x => x.training_session_id,
                        principalTable: "training_sessions",
                        principalColumn: "id",
                        onDelete: ReferentialAction.SetNull);
                });

            migrationBuilder.CreateTable(
                name: "workout_exercises",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    workout_session_id = table.Column<Guid>(type: "uuid", nullable: false),
                    exercise_id = table.Column<Guid>(type: "uuid", nullable: false),
                    exercise_slot_id = table.Column<Guid>(type: "uuid", nullable: true),
                    order_in_session = table.Column<int>(type: "integer", nullable: false),
                    notes = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true),
                    created_at_utc = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    updated_at_utc = table.Column<DateTime>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_workout_exercises", x => x.id);
                    table.ForeignKey(
                        name: "FK_workout_exercises_exercise_slots_exercise_slot_id",
                        column: x => x.exercise_slot_id,
                        principalTable: "exercise_slots",
                        principalColumn: "id",
                        onDelete: ReferentialAction.SetNull);
                    table.ForeignKey(
                        name: "FK_workout_exercises_exercises_exercise_id",
                        column: x => x.exercise_id,
                        principalTable: "exercises",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_workout_exercises_workout_sessions_workout_session_id",
                        column: x => x.workout_session_id,
                        principalTable: "workout_sessions",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "workout_sets",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    workout_exercise_id = table.Column<Guid>(type: "uuid", nullable: false),
                    set_number = table.Column<int>(type: "integer", nullable: false),
                    repetitions = table.Column<int>(type: "integer", nullable: false),
                    load_kg = table.Column<decimal>(type: "numeric(6,2)", precision: 6, scale: 2, nullable: false),
                    rir = table.Column<decimal>(type: "numeric(3,1)", precision: 3, scale: 1, nullable: true),
                    is_completed = table.Column<bool>(type: "boolean", nullable: false),
                    notes = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true),
                    created_at_utc = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    updated_at_utc = table.Column<DateTime>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_workout_sets", x => x.id);
                    table.ForeignKey(
                        name: "FK_workout_sets_workout_exercises_workout_exercise_id",
                        column: x => x.workout_exercise_id,
                        principalTable: "workout_exercises",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_workout_exercises_exercise_id",
                table: "workout_exercises",
                column: "exercise_id");

            migrationBuilder.CreateIndex(
                name: "IX_workout_exercises_exercise_slot_id",
                table: "workout_exercises",
                column: "exercise_slot_id");

            migrationBuilder.CreateIndex(
                name: "IX_workout_exercises_workout_session_id",
                table: "workout_exercises",
                column: "workout_session_id");

            migrationBuilder.CreateIndex(
                name: "IX_workout_sessions_client_id",
                table: "workout_sessions",
                column: "client_id");

            migrationBuilder.CreateIndex(
                name: "IX_workout_sessions_coach_id",
                table: "workout_sessions",
                column: "coach_id");

            migrationBuilder.CreateIndex(
                name: "IX_workout_sessions_program_version_id",
                table: "workout_sessions",
                column: "program_version_id");

            migrationBuilder.CreateIndex(
                name: "IX_workout_sessions_started_at_utc",
                table: "workout_sessions",
                column: "started_at_utc");

            migrationBuilder.CreateIndex(
                name: "IX_workout_sessions_status",
                table: "workout_sessions",
                column: "status");

            migrationBuilder.CreateIndex(
                name: "IX_workout_sessions_training_session_id",
                table: "workout_sessions",
                column: "training_session_id");

            migrationBuilder.CreateIndex(
                name: "IX_workout_sets_workout_exercise_id",
                table: "workout_sets",
                column: "workout_exercise_id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "workout_sets");

            migrationBuilder.DropTable(
                name: "workout_exercises");

            migrationBuilder.DropTable(
                name: "workout_sessions");
        }
    }
}
