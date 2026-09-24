using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace AiCoachOs.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class AddM11EgyptLocalization : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<Guid>(
                name: "gym_profile_id",
                table: "client_training_profiles",
                type: "uuid",
                nullable: true);

            migrationBuilder.CreateTable(
                name: "GymProfiles",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    CoachId = table.Column<Guid>(type: "uuid", nullable: false),
                    Name = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    Location = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true),
                    Tier = table.Column<int>(type: "integer", nullable: false),
                    ExplicitEquipmentIdsJson = table.Column<string>(type: "text", nullable: false),
                    CreatedAtUtc = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    UpdatedAtUtc = table.Column<DateTime>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_GymProfiles", x => x.Id);
                });

            migrationBuilder.CreateIndex(
                name: "IX_client_training_profiles_gym_profile_id",
                table: "client_training_profiles",
                column: "gym_profile_id");

            migrationBuilder.CreateIndex(
                name: "IX_GymProfiles_CoachId",
                table: "GymProfiles",
                column: "CoachId");

            migrationBuilder.CreateIndex(
                name: "IX_GymProfiles_Tier",
                table: "GymProfiles",
                column: "Tier");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "GymProfiles");

            migrationBuilder.DropIndex(
                name: "IX_client_training_profiles_gym_profile_id",
                table: "client_training_profiles");

            migrationBuilder.DropColumn(
                name: "gym_profile_id",
                table: "client_training_profiles");
        }
    }
}
