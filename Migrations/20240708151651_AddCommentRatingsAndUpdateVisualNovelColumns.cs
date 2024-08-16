using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace VNAPI.Migrations
{
    /// <inheritdoc />
    public partial class AddCommentRatingsAndUpdateVisualNovelColumns : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "SoundtrackYoutubePlaylistLink",
                table: "VisualNovels",
                type: "text",
                nullable: true);

            migrationBuilder.CreateTable(
                name: "AnimeLinks",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    VisualNovelId = table.Column<int>(type: "integer", nullable: true),
                    Name = table.Column<string>(type: "text", nullable: false),
                    Url = table.Column<string>(type: "text", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AnimeLinks", x => x.Id);
                    table.ForeignKey(
                        name: "FK_AnimeLinks_VisualNovels_VisualNovelId",
                        column: x => x.VisualNovelId,
                        principalTable: "VisualNovels",
                        principalColumn: "Id");
                });

            migrationBuilder.CreateTable(
                name: "CommentRatings",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    UserId = table.Column<Guid>(type: "uuid", nullable: false),
                    CommentId = table.Column<Guid>(type: "uuid", nullable: false),
                    IsLike = table.Column<bool>(type: "boolean", nullable: false),
                    AddedDate = table.Column<DateTime>(type: "timestamp without time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_CommentRatings", x => x.Id);
                });

            migrationBuilder.CreateIndex(
                name: "IX_AnimeLinks_Id",
                table: "AnimeLinks",
                column: "Id",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_AnimeLinks_VisualNovelId",
                table: "AnimeLinks",
                column: "VisualNovelId");

            migrationBuilder.CreateIndex(
                name: "IX_CommentRatings_CommentId",
                table: "CommentRatings",
                column: "CommentId");

            migrationBuilder.CreateIndex(
                name: "IX_CommentRatings_Id",
                table: "CommentRatings",
                column: "Id",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_CommentRatings_UserId",
                table: "CommentRatings",
                column: "UserId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "AnimeLinks");

            migrationBuilder.DropTable(
                name: "CommentRatings");

            migrationBuilder.DropColumn(
                name: "SoundtrackYoutubePlaylistLink",
                table: "VisualNovels");
        }
    }
}
