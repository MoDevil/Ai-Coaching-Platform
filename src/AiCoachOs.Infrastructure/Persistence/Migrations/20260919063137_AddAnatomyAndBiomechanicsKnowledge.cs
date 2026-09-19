using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace AiCoachOs.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class AddAnatomyAndBiomechanicsKnowledge : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "anatomical_regions",
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
                    table.PrimaryKey("PK_anatomical_regions", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "biomechanical_considerations",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    exercise_id = table.Column<Guid>(type: "uuid", nullable: false),
                    aspect = table.Column<int>(type: "integer", nullable: false),
                    certainty = table.Column<int>(type: "integer", nullable: false),
                    summary = table.Column<string>(type: "character varying(300)", maxLength: 300, nullable: false),
                    explanation = table.Column<string>(type: "character varying(2000)", maxLength: 2000, nullable: false),
                    practical_cues = table.Column<string>(type: "character varying(1000)", maxLength: 1000, nullable: true),
                    knowledge_claim_id = table.Column<Guid>(type: "uuid", nullable: true),
                    created_at_utc = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    updated_at_utc = table.Column<DateTime>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_biomechanical_considerations", x => x.id);
                    table.ForeignKey(
                        name: "FK_biomechanical_considerations_exercises_exercise_id",
                        column: x => x.exercise_id,
                        principalTable: "exercises",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_biomechanical_considerations_knowledge_claims_knowledge_cla~",
                        column: x => x.knowledge_claim_id,
                        principalTable: "knowledge_claims",
                        principalColumn: "id",
                        onDelete: ReferentialAction.SetNull);
                });

            migrationBuilder.CreateTable(
                name: "joints",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    region_id = table.Column<Guid>(type: "uuid", nullable: false),
                    name = table.Column<string>(type: "character varying(150)", maxLength: 150, nullable: false),
                    common_name = table.Column<string>(type: "character varying(150)", maxLength: 150, nullable: true),
                    description = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true),
                    created_at_utc = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    updated_at_utc = table.Column<DateTime>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_joints", x => x.id);
                    table.ForeignKey(
                        name: "FK_joints_anatomical_regions_region_id",
                        column: x => x.region_id,
                        principalTable: "anatomical_regions",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "joint_actions",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    joint_id = table.Column<Guid>(type: "uuid", nullable: false),
                    action_type = table.Column<int>(type: "integer", nullable: false),
                    plane_of_motion = table.Column<int>(type: "integer", nullable: false),
                    description = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true),
                    created_at_utc = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    updated_at_utc = table.Column<DateTime>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_joint_actions", x => x.id);
                    table.ForeignKey(
                        name: "FK_joint_actions_joints_joint_id",
                        column: x => x.joint_id,
                        principalTable: "joints",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "exercise_joint_actions",
                columns: table => new
                {
                    exercise_id = table.Column<Guid>(type: "uuid", nullable: false),
                    joint_action_id = table.Column<Guid>(type: "uuid", nullable: false),
                    role = table.Column<int>(type: "integer", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_exercise_joint_actions", x => new { x.exercise_id, x.joint_action_id });
                    table.ForeignKey(
                        name: "FK_exercise_joint_actions_exercises_exercise_id",
                        column: x => x.exercise_id,
                        principalTable: "exercises",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_exercise_joint_actions_joint_actions_joint_action_id",
                        column: x => x.joint_action_id,
                        principalTable: "joint_actions",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "muscle_joint_actions",
                columns: table => new
                {
                    muscle_id = table.Column<Guid>(type: "uuid", nullable: false),
                    joint_action_id = table.Column<Guid>(type: "uuid", nullable: false),
                    is_primary_action = table.Column<bool>(type: "boolean", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_muscle_joint_actions", x => new { x.muscle_id, x.joint_action_id });
                    table.ForeignKey(
                        name: "FK_muscle_joint_actions_joint_actions_joint_action_id",
                        column: x => x.joint_action_id,
                        principalTable: "joint_actions",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_muscle_joint_actions_muscles_muscle_id",
                        column: x => x.muscle_id,
                        principalTable: "muscles",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_anatomical_regions_name",
                table: "anatomical_regions",
                column: "name",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_biomechanical_considerations_aspect",
                table: "biomechanical_considerations",
                column: "aspect");

            migrationBuilder.CreateIndex(
                name: "IX_biomechanical_considerations_certainty",
                table: "biomechanical_considerations",
                column: "certainty");

            migrationBuilder.CreateIndex(
                name: "IX_biomechanical_considerations_exercise_id",
                table: "biomechanical_considerations",
                column: "exercise_id");

            migrationBuilder.CreateIndex(
                name: "IX_biomechanical_considerations_knowledge_claim_id",
                table: "biomechanical_considerations",
                column: "knowledge_claim_id");

            migrationBuilder.CreateIndex(
                name: "IX_exercise_joint_actions_joint_action_id",
                table: "exercise_joint_actions",
                column: "joint_action_id");

            migrationBuilder.CreateIndex(
                name: "IX_joint_actions_joint_id_action_type",
                table: "joint_actions",
                columns: new[] { "joint_id", "action_type" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_joints_name",
                table: "joints",
                column: "name");

            migrationBuilder.CreateIndex(
                name: "IX_joints_region_id",
                table: "joints",
                column: "region_id");

            migrationBuilder.CreateIndex(
                name: "IX_muscle_joint_actions_joint_action_id",
                table: "muscle_joint_actions",
                column: "joint_action_id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "biomechanical_considerations");

            migrationBuilder.DropTable(
                name: "exercise_joint_actions");

            migrationBuilder.DropTable(
                name: "muscle_joint_actions");

            migrationBuilder.DropTable(
                name: "joint_actions");

            migrationBuilder.DropTable(
                name: "joints");

            migrationBuilder.DropTable(
                name: "anatomical_regions");
        }
    }
}
