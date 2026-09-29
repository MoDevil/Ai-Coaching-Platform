using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace AiCoachOs.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class AlignM15ClientPhotoSchema : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_client_photos_taken_at",
                table: "client_photos");

            migrationBuilder.DropColumn(
                name: "taken_at",
                table: "client_photos");

            migrationBuilder.CreateIndex(
                name: "IX_client_photos_uploaded_at",
                table: "client_photos",
                column: "uploaded_at");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_client_photos_uploaded_at",
                table: "client_photos");

            migrationBuilder.AddColumn<DateTime>(
                name: "taken_at",
                table: "client_photos",
                type: "timestamp with time zone",
                nullable: false,
                defaultValue: new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified));

            migrationBuilder.CreateIndex(
                name: "IX_client_photos_taken_at",
                table: "client_photos",
                column: "taken_at");
        }
    }
}
