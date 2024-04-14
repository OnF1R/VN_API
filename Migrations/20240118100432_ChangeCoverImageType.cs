using System;
using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace VNAPI.Migrations
{
    /// <inheritdoc />
    public partial class ChangeCoverImageType : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>
                (name: "CoverImagePath",
                table: "VisualNovels",
                nullable: true);

            migrationBuilder.DropColumn(
                name: "CoverImage",
                table: "VisualNovels");
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
                name: "TagsMetadata");

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

            migrationBuilder.DropTable(
                name: "Authors");

            migrationBuilder.DropTable(
                name: "Translators");
        }
    }
}
