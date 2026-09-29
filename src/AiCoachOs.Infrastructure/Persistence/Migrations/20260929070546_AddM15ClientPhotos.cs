using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace AiCoachOs.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class AddM15ClientPhotos : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "client_photos",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    client_id = table.Column<Guid>(type: "uuid", nullable: false),
                    coach_id = table.Column<Guid>(type: "uuid", nullable: false),
                    photo_set_type = table.Column<int>(type: "integer", nullable: false),
                    storage_key = table.Column<string>(type: "character varying(250)", maxLength: 250, nullable: false),
                    mime_type = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    file_size_bytes = table.Column<long>(type: "bigint", nullable: false),
                    taken_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    uploaded_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    notes = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true),
                    observation_record_id = table.Column<Guid>(type: "uuid", nullable: true),
                    is_anonymized = table.Column<bool>(type: "boolean", nullable: false),
                    anonymized_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    created_at_utc = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    updated_at_utc = table.Column<DateTime>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_client_photos", x => x.id);
                    table.ForeignKey(
                        name: "FK_client_photos_ClientMemoryRecords_observation_record_id",
                        column: x => x.observation_record_id,
                        principalTable: "ClientMemoryRecords",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.SetNull);
                    table.ForeignKey(
                        name: "FK_client_photos_clients_client_id",
                        column: x => x.client_id,
                        principalTable: "clients",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(
                name: "IX_client_photos_client_id",
                table: "client_photos",
                column: "client_id");

            migrationBuilder.CreateIndex(
                name: "IX_client_photos_coach_id",
                table: "client_photos",
                column: "coach_id");

            migrationBuilder.CreateIndex(
                name: "IX_client_photos_observation_record_id",
                table: "client_photos",
                column: "observation_record_id");

            migrationBuilder.CreateIndex(
                name: "IX_client_photos_taken_at",
                table: "client_photos",
                column: "taken_at");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "client_photos");
        }
    }
}
