using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace AiCoachOs.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class AddM10NutritionFoundation : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "ClientNutritionProfiles",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    ClientId = table.Column<Guid>(type: "uuid", nullable: false),
                    BudgetTier = table.Column<int>(type: "integer", nullable: false),
                    MealsPerDay = table.Column<int>(type: "integer", nullable: true),
                    CurrentCalorieTarget = table.Column<int>(type: "integer", nullable: true),
                    CurrentProteinTargetGrams = table.Column<int>(type: "integer", nullable: true),
                    TargetSetAtUtc = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    TargetSetMethod = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: true),
                    DietaryPreferencesJson = table.Column<string>(type: "text", nullable: false),
                    FoodExclusionsJson = table.Column<string>(type: "text", nullable: false),
                    CreatedAtUtc = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    UpdatedAtUtc = table.Column<DateTime>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ClientNutritionProfiles", x => x.Id);
                    table.ForeignKey(
                        name: "FK_ClientNutritionProfiles_clients_ClientId",
                        column: x => x.ClientId,
                        principalTable: "clients",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "EgyptianFoods",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    NameAr = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    NameEn = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    ServingDescription = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    ServingGrams = table.Column<decimal>(type: "numeric(6,2)", precision: 6, scale: 2, nullable: false),
                    CaloriesPer100g = table.Column<decimal>(type: "numeric(6,2)", precision: 6, scale: 2, nullable: false),
                    ProteinPer100g = table.Column<decimal>(type: "numeric(6,2)", precision: 6, scale: 2, nullable: false),
                    CarbsPer100g = table.Column<decimal>(type: "numeric(6,2)", precision: 6, scale: 2, nullable: false),
                    FatPer100g = table.Column<decimal>(type: "numeric(6,2)", precision: 6, scale: 2, nullable: false),
                    FiberPer100g = table.Column<decimal>(type: "numeric(6,2)", precision: 6, scale: 2, nullable: true),
                    FoodCategory = table.Column<int>(type: "integer", nullable: false),
                    IsAffordableLow = table.Column<bool>(type: "boolean", nullable: false),
                    IsAffordableMid = table.Column<bool>(type: "boolean", nullable: false),
                    DataSource = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: false),
                    DataConfidence = table.Column<int>(type: "integer", nullable: false),
                    VariabilityNote = table.Column<string>(type: "character varying(1000)", maxLength: 1000, nullable: true),
                    CreatedAtUtc = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    UpdatedAtUtc = table.Column<DateTime>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_EgyptianFoods", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "NutritionCalibrationRecords",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    ClientNutritionProfileId = table.Column<Guid>(type: "uuid", nullable: false),
                    RecordedAtUtc = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    WeightKg = table.Column<decimal>(type: "numeric(5,2)", precision: 5, scale: 2, nullable: false),
                    EstimatedTDEE = table.Column<int>(type: "integer", nullable: true),
                    AdjustmentRecommendation = table.Column<int>(type: "integer", nullable: false),
                    AdjustmentKcal = table.Column<int>(type: "integer", nullable: true),
                    WeeksObserved = table.Column<int>(type: "integer", nullable: false),
                    CoachDecision = table.Column<int>(type: "integer", nullable: false),
                    CoachNote = table.Column<string>(type: "character varying(1000)", maxLength: 1000, nullable: true),
                    WeeklyWeightAveragesJson = table.Column<string>(type: "text", nullable: false),
                    CreatedAtUtc = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    UpdatedAtUtc = table.Column<DateTime>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_NutritionCalibrationRecords", x => x.Id);
                    table.ForeignKey(
                        name: "FK_NutritionCalibrationRecords_ClientNutritionProfiles_ClientN~",
                        column: x => x.ClientNutritionProfileId,
                        principalTable: "ClientNutritionProfiles",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_ClientNutritionProfiles_ClientId",
                table: "ClientNutritionProfiles",
                column: "ClientId",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_EgyptianFoods_FoodCategory",
                table: "EgyptianFoods",
                column: "FoodCategory");

            migrationBuilder.CreateIndex(
                name: "IX_EgyptianFoods_IsAffordableLow",
                table: "EgyptianFoods",
                column: "IsAffordableLow");

            migrationBuilder.CreateIndex(
                name: "IX_EgyptianFoods_IsAffordableMid",
                table: "EgyptianFoods",
                column: "IsAffordableMid");

            migrationBuilder.CreateIndex(
                name: "IX_NutritionCalibrationRecords_ClientNutritionProfileId",
                table: "NutritionCalibrationRecords",
                column: "ClientNutritionProfileId");

            migrationBuilder.CreateIndex(
                name: "IX_NutritionCalibrationRecords_RecordedAtUtc",
                table: "NutritionCalibrationRecords",
                column: "RecordedAtUtc");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "EgyptianFoods");

            migrationBuilder.DropTable(
                name: "NutritionCalibrationRecords");

            migrationBuilder.DropTable(
                name: "ClientNutritionProfiles");
        }
    }
}
