using System.Collections.Generic;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace VNAPI.Migrations
{
    /// <inheritdoc />
    public partial class AddPhotosFieldsToWorkWithS3 : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "CoverImagePath",
                table: "VisualNovels",
                newName: "CoverImageFileName");

            migrationBuilder.AddColumn<string>(
                name: "BackgroundImageFileName",
                table: "VisualNovels",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<List<string>>(
                name: "ScreenshotFileNames",
                table: "VisualNovels",
                type: "text[]",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "BackgroundImageFileName",
                table: "VisualNovels");

            migrationBuilder.DropColumn(
                name: "ScreenshotFileNames",
                table: "VisualNovels");

            migrationBuilder.RenameColumn(
                name: "CoverImageFileName",
                table: "VisualNovels",
                newName: "CoverImagePath");
        }
    }
}
