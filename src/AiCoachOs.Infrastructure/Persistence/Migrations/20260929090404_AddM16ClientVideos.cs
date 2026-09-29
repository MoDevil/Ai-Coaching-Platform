using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace AiCoachOs.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class AddM16ClientVideos : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "client_videos",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    client_id = table.Column<Guid>(type: "uuid", nullable: false),
                    coach_id = table.Column<Guid>(type: "uuid", nullable: false),
                    exercise_id = table.Column<Guid>(type: "uuid", nullable: true),
                    exercise_name = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    storage_key = table.Column<string>(type: "character varying(250)", maxLength: 250, nullable: false),
                    mime_type = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    file_size_bytes = table.Column<long>(type: "bigint", nullable: false),
                    duration_seconds = table.Column<int>(type: "integer", nullable: false),
                    frame_count = table.Column<int>(type: "integer", nullable: false),
                    frame_storage_keys = table.Column<string>(type: "jsonb", nullable: false),
                    uploaded_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    coach_notes = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true),
                    observation_record_id = table.Column<Guid>(type: "uuid", nullable: true),
                    is_anonymized = table.Column<bool>(type: "boolean", nullable: false),
                    anonymized_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    created_at_utc = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    updated_at_utc = table.Column<DateTime>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_client_videos", x => x.id);
                    table.ForeignKey(
                        name: "FK_client_videos_ClientMemoryRecords_observation_record_id",
                        column: x => x.observation_record_id,
                        principalTable: "ClientMemoryRecords",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_client_videos_clients_client_id",
                        column: x => x.client_id,
                        principalTable: "clients",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_client_videos_exercises_exercise_id",
                        column: x => x.exercise_id,
                        principalTable: "exercises",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(
                name: "IX_client_videos_client_id",
                table: "client_videos",
                column: "client_id");

            migrationBuilder.CreateIndex(
                name: "IX_client_videos_coach_id",
                table: "client_videos",
                column: "coach_id");

            migrationBuilder.CreateIndex(
                name: "IX_client_videos_exercise_id",
                table: "client_videos",
                column: "exercise_id");

            migrationBuilder.CreateIndex(
                name: "IX_client_videos_observation_record_id",
                table: "client_videos",
                column: "observation_record_id");

            migrationBuilder.CreateIndex(
                name: "IX_client_videos_uploaded_at",
                table: "client_videos",
                column: "uploaded_at");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "client_videos");
        }
    }
}
