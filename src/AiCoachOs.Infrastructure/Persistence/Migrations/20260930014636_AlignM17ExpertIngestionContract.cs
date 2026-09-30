using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace AiCoachOs.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class AlignM17ExpertIngestionContract : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_expert_content_ingestions_expert_sources_source_id",
                table: "expert_content_ingestions");

            migrationBuilder.DropIndex(
                name: "IX_expert_sources_credibility_tier",
                table: "expert_sources");

            migrationBuilder.DropIndex(
                name: "IX_expert_claims_review_status",
                table: "expert_claims");

            migrationBuilder.DropIndex(
                name: "IX_expert_claims_topic",
                table: "expert_claims");

            migrationBuilder.DropColumn(
                name: "bio",
                table: "expert_sources");

            migrationBuilder.DropColumn(
                name: "channel_or_publication",
                table: "expert_sources");

            migrationBuilder.DropColumn(
                name: "credibility_tier",
                table: "expert_sources");

            migrationBuilder.DropColumn(
                name: "primary_domain",
                table: "expert_sources");

            migrationBuilder.DropColumn(
                name: "medical_warning_acknowledged",
                table: "expert_content_ingestions");

            migrationBuilder.DropColumn(
                name: "raw_extracted_text_snippet",
                table: "expert_content_ingestions");

            migrationBuilder.DropColumn(
                name: "context_or_timestamp",
                table: "expert_claims");

            migrationBuilder.DropColumn(
                name: "sub_topic",
                table: "expert_claims");

            migrationBuilder.DropColumn(
                name: "topic",
                table: "expert_claims");

            migrationBuilder.RenameColumn(
                name: "platform",
                table: "expert_sources",
                newName: "source_type");

            migrationBuilder.RenameIndex(
                name: "IX_expert_sources_platform",
                table: "expert_sources",
                newName: "IX_expert_sources_source_type");

            migrationBuilder.RenameColumn(
                name: "word_count",
                table: "expert_content_ingestions",
                newName: "source_type");

            migrationBuilder.RenameColumn(
                name: "title",
                table: "expert_content_ingestions",
                newName: "source_title");

            migrationBuilder.RenameColumn(
                name: "source_id",
                table: "expert_content_ingestions",
                newName: "expert_source_id");

            migrationBuilder.RenameColumn(
                name: "content_type",
                table: "expert_content_ingestions",
                newName: "extracted_text_length");

            migrationBuilder.RenameColumn(
                name: "completed_at_utc",
                table: "expert_content_ingestions",
                newName: "published_at");

            migrationBuilder.RenameIndex(
                name: "IX_expert_content_ingestions_source_id",
                table: "expert_content_ingestions",
                newName: "IX_expert_content_ingestions_expert_source_id");

            migrationBuilder.RenameColumn(
                name: "reviewed_at_utc",
                table: "expert_claims",
                newName: "coach_reviewed_at");

            migrationBuilder.RenameColumn(
                name: "review_status",
                table: "expert_claims",
                newName: "evidence_classification");

            migrationBuilder.RenameColumn(
                name: "nature_of_claim",
                table: "expert_claims",
                newName: "creator_confidence");

            migrationBuilder.RenameColumn(
                name: "coach_notes",
                table: "expert_claims",
                newName: "coach_note");

            migrationBuilder.AddColumn<string>(
                name: "url",
                table: "expert_sources",
                type: "character varying(1000)",
                maxLength: 1000,
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<DateTime>(
                name: "processed_at_utc",
                table: "expert_content_ingestions",
                type: "timestamp with time zone",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "claim_category",
                table: "expert_claims",
                type: "integer",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<int>(
                name: "coach_review_status",
                table: "expert_claims",
                type: "integer",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<string>(
                name: "source_context",
                table: "expert_claims",
                type: "character varying(500)",
                maxLength: 500,
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_expert_claims_claim_category",
                table: "expert_claims",
                column: "claim_category");

            migrationBuilder.CreateIndex(
                name: "IX_expert_claims_coach_review_status",
                table: "expert_claims",
                column: "coach_review_status");

            migrationBuilder.AddForeignKey(
                name: "FK_expert_content_ingestions_expert_sources_expert_source_id",
                table: "expert_content_ingestions",
                column: "expert_source_id",
                principalTable: "expert_sources",
                principalColumn: "id",
                onDelete: ReferentialAction.Restrict);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_expert_content_ingestions_expert_sources_expert_source_id",
                table: "expert_content_ingestions");

            migrationBuilder.DropIndex(
                name: "IX_expert_claims_claim_category",
                table: "expert_claims");

            migrationBuilder.DropIndex(
                name: "IX_expert_claims_coach_review_status",
                table: "expert_claims");

            migrationBuilder.DropColumn(
                name: "url",
                table: "expert_sources");

            migrationBuilder.DropColumn(
                name: "processed_at_utc",
                table: "expert_content_ingestions");

            migrationBuilder.DropColumn(
                name: "claim_category",
                table: "expert_claims");

            migrationBuilder.DropColumn(
                name: "coach_review_status",
                table: "expert_claims");

            migrationBuilder.DropColumn(
                name: "source_context",
                table: "expert_claims");

            migrationBuilder.RenameColumn(
                name: "source_type",
                table: "expert_sources",
                newName: "platform");

            migrationBuilder.RenameIndex(
                name: "IX_expert_sources_source_type",
                table: "expert_sources",
                newName: "IX_expert_sources_platform");

            migrationBuilder.RenameColumn(
                name: "source_type",
                table: "expert_content_ingestions",
                newName: "word_count");

            migrationBuilder.RenameColumn(
                name: "source_title",
                table: "expert_content_ingestions",
                newName: "title");

            migrationBuilder.RenameColumn(
                name: "published_at",
                table: "expert_content_ingestions",
                newName: "completed_at_utc");

            migrationBuilder.RenameColumn(
                name: "extracted_text_length",
                table: "expert_content_ingestions",
                newName: "content_type");

            migrationBuilder.RenameColumn(
                name: "expert_source_id",
                table: "expert_content_ingestions",
                newName: "source_id");

            migrationBuilder.RenameIndex(
                name: "IX_expert_content_ingestions_expert_source_id",
                table: "expert_content_ingestions",
                newName: "IX_expert_content_ingestions_source_id");

            migrationBuilder.RenameColumn(
                name: "evidence_classification",
                table: "expert_claims",
                newName: "review_status");

            migrationBuilder.RenameColumn(
                name: "creator_confidence",
                table: "expert_claims",
                newName: "nature_of_claim");

            migrationBuilder.RenameColumn(
                name: "coach_reviewed_at",
                table: "expert_claims",
                newName: "reviewed_at_utc");

            migrationBuilder.RenameColumn(
                name: "coach_note",
                table: "expert_claims",
                newName: "coach_notes");

            migrationBuilder.AddColumn<string>(
                name: "bio",
                table: "expert_sources",
                type: "character varying(2000)",
                maxLength: 2000,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "channel_or_publication",
                table: "expert_sources",
                type: "character varying(200)",
                maxLength: 200,
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<int>(
                name: "credibility_tier",
                table: "expert_sources",
                type: "integer",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<string>(
                name: "primary_domain",
                table: "expert_sources",
                type: "character varying(200)",
                maxLength: 200,
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<bool>(
                name: "medical_warning_acknowledged",
                table: "expert_content_ingestions",
                type: "boolean",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<string>(
                name: "raw_extracted_text_snippet",
                table: "expert_content_ingestions",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "context_or_timestamp",
                table: "expert_claims",
                type: "character varying(200)",
                maxLength: 200,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "sub_topic",
                table: "expert_claims",
                type: "character varying(100)",
                maxLength: 100,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "topic",
                table: "expert_claims",
                type: "character varying(100)",
                maxLength: 100,
                nullable: false,
                defaultValue: "");

            migrationBuilder.CreateIndex(
                name: "IX_expert_sources_credibility_tier",
                table: "expert_sources",
                column: "credibility_tier");

            migrationBuilder.CreateIndex(
                name: "IX_expert_claims_review_status",
                table: "expert_claims",
                column: "review_status");

            migrationBuilder.CreateIndex(
                name: "IX_expert_claims_topic",
                table: "expert_claims",
                column: "topic");

            migrationBuilder.AddForeignKey(
                name: "FK_expert_content_ingestions_expert_sources_source_id",
                table: "expert_content_ingestions",
                column: "source_id",
                principalTable: "expert_sources",
                principalColumn: "id",
                onDelete: ReferentialAction.Restrict);
        }
    }
}
