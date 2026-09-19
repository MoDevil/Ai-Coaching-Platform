using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace AiCoachOs.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class AddExerciseAndTrainingProfile : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "client_training_profiles",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    client_id = table.Column<Guid>(type: "uuid", nullable: false),
                    experience_level = table.Column<int>(type: "integer", nullable: false),
                    session_duration_min_minutes = table.Column<int>(type: "integer", nullable: true),
                    session_duration_target_minutes = table.Column<int>(type: "integer", nullable: true),
                    session_duration_max_minutes = table.Column<int>(type: "integer", nullable: true),
                    sessions_per_week = table.Column<int>(type: "integer", nullable: false),
                    available_days = table.Column<string>(type: "text", nullable: false),
                    preferred_days = table.Column<string>(type: "text", nullable: false),
                    exercise_preferences = table.Column<string>(type: "character varying(2000)", maxLength: 2000, nullable: true),
                    exercise_constraints = table.Column<string>(type: "character varying(2000)", maxLength: 2000, nullable: true),
                    available_equipment_ids = table.Column<string>(type: "text", nullable: false),
                    created_at_utc = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    updated_at_utc = table.Column<DateTime>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_client_training_profiles", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "equipment",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    name = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    category = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    created_at_utc = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    updated_at_utc = table.Column<DateTime>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_equipment", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "movement_patterns",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    name = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    description = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true),
                    created_at_utc = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    updated_at_utc = table.Column<DateTime>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_movement_patterns", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "muscles",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    name = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    common_name = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    body_part = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    created_at_utc = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    updated_at_utc = table.Column<DateTime>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_muscles", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "client_training_priorities",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    profile_id = table.Column<Guid>(type: "uuid", nullable: false),
                    order = table.Column<int>(type: "integer", nullable: false),
                    focus_area = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    notes = table.Column<string>(type: "character varying(1000)", maxLength: 1000, nullable: true),
                    created_at_utc = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    updated_at_utc = table.Column<DateTime>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_client_training_priorities", x => x.id);
                    table.ForeignKey(
                        name: "FK_client_training_priorities_client_training_profiles_profile~",
                        column: x => x.profile_id,
                        principalTable: "client_training_profiles",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "exercises",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    name = table.Column<string>(type: "character varying(150)", maxLength: 150, nullable: false),
                    aliases = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true),
                    category = table.Column<int>(type: "integer", nullable: false),
                    movement_pattern_id = table.Column<Guid>(type: "uuid", nullable: false),
                    joint_actions = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true),
                    stability_requirement = table.Column<int>(type: "integer", nullable: false),
                    technical_demand = table.Column<int>(type: "integer", nullable: false),
                    local_fatigue_cost = table.Column<int>(type: "integer", nullable: false),
                    systemic_fatigue_cost = table.Column<int>(type: "integer", nullable: false),
                    stimulus_potential = table.Column<int>(type: "integer", nullable: false),
                    progression_potential = table.Column<int>(type: "integer", nullable: false),
                    resistance_profile = table.Column<int>(type: "integer", nullable: false),
                    substitution_group_id = table.Column<Guid>(type: "uuid", nullable: true),
                    created_at_utc = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    updated_at_utc = table.Column<DateTime>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_exercises", x => x.id);
                    table.ForeignKey(
                        name: "FK_exercises_movement_patterns_movement_pattern_id",
                        column: x => x.movement_pattern_id,
                        principalTable: "movement_patterns",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "exercise_equipment",
                columns: table => new
                {
                    exercise_id = table.Column<Guid>(type: "uuid", nullable: false),
                    equipment_id = table.Column<Guid>(type: "uuid", nullable: false),
                    is_required = table.Column<bool>(type: "boolean", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_exercise_equipment", x => new { x.exercise_id, x.equipment_id });
                    table.ForeignKey(
                        name: "FK_exercise_equipment_equipment_equipment_id",
                        column: x => x.equipment_id,
                        principalTable: "equipment",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_exercise_equipment_exercises_exercise_id",
                        column: x => x.exercise_id,
                        principalTable: "exercises",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "exercise_muscles",
                columns: table => new
                {
                    exercise_id = table.Column<Guid>(type: "uuid", nullable: false),
                    muscle_id = table.Column<Guid>(type: "uuid", nullable: false),
                    is_primary = table.Column<bool>(type: "boolean", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_exercise_muscles", x => new { x.exercise_id, x.muscle_id });
                    table.ForeignKey(
                        name: "FK_exercise_muscles_exercises_exercise_id",
                        column: x => x.exercise_id,
                        principalTable: "exercises",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_exercise_muscles_muscles_muscle_id",
                        column: x => x.muscle_id,
                        principalTable: "muscles",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "exercise_substitutions",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    exercise_id = table.Column<Guid>(type: "uuid", nullable: false),
                    substitute_exercise_id = table.Column<Guid>(type: "uuid", nullable: false),
                    intent_preservation_notes = table.Column<string>(type: "character varying(1000)", maxLength: 1000, nullable: true),
                    created_at_utc = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    updated_at_utc = table.Column<DateTime>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_exercise_substitutions", x => x.id);
                    table.ForeignKey(
                        name: "FK_exercise_substitutions_exercises_exercise_id",
                        column: x => x.exercise_id,
                        principalTable: "exercises",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_exercise_substitutions_exercises_substitute_exercise_id",
                        column: x => x.substitute_exercise_id,
                        principalTable: "exercises",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(
                name: "IX_client_training_priorities_profile_id_order",
                table: "client_training_priorities",
                columns: new[] { "profile_id", "order" });

            migrationBuilder.CreateIndex(
                name: "IX_client_training_profiles_client_id",
                table: "client_training_profiles",
                column: "client_id",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_exercise_equipment_equipment_id",
                table: "exercise_equipment",
                column: "equipment_id");

            migrationBuilder.CreateIndex(
                name: "IX_exercise_muscles_muscle_id",
                table: "exercise_muscles",
                column: "muscle_id");

            migrationBuilder.CreateIndex(
                name: "IX_exercise_substitutions_exercise_id",
                table: "exercise_substitutions",
                column: "exercise_id");

            migrationBuilder.CreateIndex(
                name: "IX_exercise_substitutions_substitute_exercise_id",
                table: "exercise_substitutions",
                column: "substitute_exercise_id");

            migrationBuilder.CreateIndex(
                name: "IX_exercises_movement_pattern_id",
                table: "exercises",
                column: "movement_pattern_id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "client_training_priorities");

            migrationBuilder.DropTable(
                name: "exercise_equipment");

            migrationBuilder.DropTable(
                name: "exercise_muscles");

            migrationBuilder.DropTable(
                name: "exercise_substitutions");

            migrationBuilder.DropTable(
                name: "client_training_profiles");

            migrationBuilder.DropTable(
                name: "equipment");

            migrationBuilder.DropTable(
                name: "muscles");

            migrationBuilder.DropTable(
                name: "exercises");

            migrationBuilder.DropTable(
                name: "movement_patterns");
        }
    }
}
