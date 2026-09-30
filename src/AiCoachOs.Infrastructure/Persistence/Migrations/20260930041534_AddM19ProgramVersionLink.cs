using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace AiCoachOs.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class AddM19ProgramVersionLink : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<Guid>(
                name: "ImplementedProgramVersionId",
                table: "AIRecommendationRecords",
                type: "uuid",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_AIRecommendationRecords_ImplementedProgramVersionId",
                table: "AIRecommendationRecords",
                column: "ImplementedProgramVersionId");

            migrationBuilder.AddForeignKey(
                name: "FK_AIRecommendationRecords_program_versions_ImplementedProgram~",
                table: "AIRecommendationRecords",
                column: "ImplementedProgramVersionId",
                principalTable: "program_versions",
                principalColumn: "id",
                onDelete: ReferentialAction.Restrict);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_AIRecommendationRecords_program_versions_ImplementedProgram~",
                table: "AIRecommendationRecords");

            migrationBuilder.DropIndex(
                name: "IX_AIRecommendationRecords_ImplementedProgramVersionId",
                table: "AIRecommendationRecords");

            migrationBuilder.DropColumn(
                name: "ImplementedProgramVersionId",
                table: "AIRecommendationRecords");
        }
    }
}
