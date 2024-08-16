using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace VNAPI.Migrations
{
    /// <inheritdoc />
    public partial class AddIndexes : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateIndex(
                name: "IX_VisualNovels_Id",
                table: "VisualNovels",
                column: "Id",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_VisualNovels_OriginalTitle",
                table: "VisualNovels",
                column: "OriginalTitle");

            migrationBuilder.CreateIndex(
                name: "IX_VisualNovels_Title",
                table: "VisualNovels",
                column: "Title");

            migrationBuilder.CreateIndex(
                name: "IX_VisualNovelLists_Id",
                table: "VisualNovelLists",
                column: "Id",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_VisualNovelLists_UserId",
                table: "VisualNovelLists",
                column: "UserId");

            migrationBuilder.CreateIndex(
                name: "IX_VisualNovelListEntries_Id",
                table: "VisualNovelListEntries",
                column: "Id",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Tags_Id",
                table: "Tags",
                column: "Id",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Rating_Id",
                table: "Rating",
                column: "Id",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Rating_UserId",
                table: "Rating",
                column: "UserId");

            migrationBuilder.CreateIndex(
                name: "IX_Rating_VisualNovelId",
                table: "Rating",
                column: "VisualNovelId");

            migrationBuilder.CreateIndex(
                name: "IX_OtherLinks_Id",
                table: "OtherLinks",
                column: "Id",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_DownloadLinks_Id",
                table: "DownloadLinks",
                column: "Id",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Authors_Id",
                table: "Authors",
                column: "Id",
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_VisualNovels_Id",
                table: "VisualNovels");

            migrationBuilder.DropIndex(
                name: "IX_VisualNovels_OriginalTitle",
                table: "VisualNovels");

            migrationBuilder.DropIndex(
                name: "IX_VisualNovels_Title",
                table: "VisualNovels");

            migrationBuilder.DropIndex(
                name: "IX_VisualNovelLists_Id",
                table: "VisualNovelLists");

            migrationBuilder.DropIndex(
                name: "IX_VisualNovelLists_UserId",
                table: "VisualNovelLists");

            migrationBuilder.DropIndex(
                name: "IX_VisualNovelListEntries_Id",
                table: "VisualNovelListEntries");

            migrationBuilder.DropIndex(
                name: "IX_Tags_Id",
                table: "Tags");

            migrationBuilder.DropIndex(
                name: "IX_Rating_Id",
                table: "Rating");

            migrationBuilder.DropIndex(
                name: "IX_Rating_UserId",
                table: "Rating");

            migrationBuilder.DropIndex(
                name: "IX_Rating_VisualNovelId",
                table: "Rating");

            migrationBuilder.DropIndex(
                name: "IX_OtherLinks_Id",
                table: "OtherLinks");

            migrationBuilder.DropIndex(
                name: "IX_DownloadLinks_Id",
                table: "DownloadLinks");

            migrationBuilder.DropIndex(
                name: "IX_Authors_Id",
                table: "Authors");
        }
    }
}
