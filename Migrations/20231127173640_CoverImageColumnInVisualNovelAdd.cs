using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace VNAPI.Migrations
{
    /// <inheritdoc />
    public partial class CoverImageColumnInVisualNovelAdd : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<byte[]>
                (name: "CoverImage",
                table: "VisualNovels",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "GamingPlatformVisualNovel");

            migrationBuilder.DropTable(
                name: "GenreVisualNovel");

            migrationBuilder.DropTable(
                name: "LanguageVisualNovel");

            migrationBuilder.DropTable(
                name: "TagVisualNovel");

            migrationBuilder.DropTable(
                name: "GamingPlatforms");

            migrationBuilder.DropTable(
                name: "Genres");

            migrationBuilder.DropTable(
                name: "Languages");

            migrationBuilder.DropTable(
                name: "Tags");

            migrationBuilder.DropTable(
                name: "VisualNovels");
        }
    }
}
