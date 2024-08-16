using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace VNAPI.Migrations
{
    /// <inheritdoc />
    public partial class AddCommentsAndAddVisualNovelColumns : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "CommentsCount",
                table: "VisualNovels",
                type: "integer",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<int>(
                name: "PageViewesCount",
                table: "VisualNovels",
                type: "integer",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.CreateTable(
                name: "Comments",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    UserId = table.Column<Guid>(type: "uuid", nullable: false),
                    VisualNovelId = table.Column<int>(type: "integer", nullable: false),
                    PostedDate = table.Column<DateTime>(type: "timestamp without time zone", nullable: false),
                    Content = table.Column<string>(type: "text", nullable: false),
                    ParentCommentId = table.Column<Guid>(type: "uuid", nullable: true),
                    VisualNovelCommentId = table.Column<Guid>(type: "uuid", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Comments", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Comments_Comments_VisualNovelCommentId",
                        column: x => x.VisualNovelCommentId,
                        principalTable: "Comments",
                        principalColumn: "Id");
                });

            migrationBuilder.CreateIndex(
                name: "IX_Comments_Id",
                table: "Comments",
                column: "Id",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Comments_UserId",
                table: "Comments",
                column: "UserId");

            migrationBuilder.CreateIndex(
                name: "IX_Comments_VisualNovelCommentId",
                table: "Comments",
                column: "VisualNovelCommentId");

            migrationBuilder.CreateIndex(
                name: "IX_Comments_VisualNovelId",
                table: "Comments",
                column: "VisualNovelId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "Comments");

            migrationBuilder.DropColumn(
                name: "CommentsCount",
                table: "VisualNovels");

            migrationBuilder.DropColumn(
                name: "PageViewesCount",
                table: "VisualNovels");
        }
    }
}
