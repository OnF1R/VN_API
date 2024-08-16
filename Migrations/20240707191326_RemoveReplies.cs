using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace VNAPI.Migrations
{
    /// <inheritdoc />
    public partial class RemoveReplies : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Comments_Comments_VisualNovelCommentId",
                table: "Comments");

            migrationBuilder.DropIndex(
                name: "IX_Comments_VisualNovelCommentId",
                table: "Comments");

            migrationBuilder.DropColumn(
                name: "VisualNovelCommentId",
                table: "Comments");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<Guid>(
                name: "VisualNovelCommentId",
                table: "Comments",
                type: "uuid",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_Comments_VisualNovelCommentId",
                table: "Comments",
                column: "VisualNovelCommentId");

            migrationBuilder.AddForeignKey(
                name: "FK_Comments_Comments_VisualNovelCommentId",
                table: "Comments",
                column: "VisualNovelCommentId",
                principalTable: "Comments",
                principalColumn: "Id");
        }
    }
}
