using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace VNAPI.Migrations
{
    /// <inheritdoc />
    public partial class ChangeImagesFileNameToLinks : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "ScreenshotFileNames",
                table: "VisualNovels",
                newName: "ScreenshotLinks");

            migrationBuilder.RenameColumn(
                name: "CoverImageFileName",
                table: "VisualNovels",
                newName: "CoverImageLink");

            migrationBuilder.RenameColumn(
                name: "BackgroundImageFileName",
                table: "VisualNovels",
                newName: "BackgroundImageLink");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "ScreenshotLinks",
                table: "VisualNovels",
                newName: "ScreenshotFileNames");

            migrationBuilder.RenameColumn(
                name: "CoverImageLink",
                table: "VisualNovels",
                newName: "CoverImageFileName");

            migrationBuilder.RenameColumn(
                name: "BackgroundImageLink",
                table: "VisualNovels",
                newName: "BackgroundImageFileName");
        }
    }
}
