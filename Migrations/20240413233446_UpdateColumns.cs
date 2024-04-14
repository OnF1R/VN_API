using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace VNAPI.Migrations
{
    /// <inheritdoc />
    public partial class UpdateColumns : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_VisualNovels_Authors_AuthorId",
                table: "VisualNovels");

            migrationBuilder.DropColumn(
                name: "Autor",
                table: "VisualNovels");

            migrationBuilder.DropColumn(
                name: "Translator",
                table: "VisualNovels");

            migrationBuilder.AlterColumn<int>(
                name: "AuthorId",
                table: "VisualNovels",
                type: "integer",
                nullable: false,
                defaultValue: 0,
                oldClrType: typeof(int),
                oldType: "integer",
                oldNullable: true);

            migrationBuilder.AddColumn<int>(
                name: "Status",
                table: "VisualNovels",
                type: "integer",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<string>(
                name: "VndbId",
                table: "Authors",
                type: "text",
                nullable: true);

            migrationBuilder.CreateTable(
                name: "DownloadLink",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    VisualNovelId = table.Column<int>(type: "integer", nullable: false),
                    GamingPlatformId = table.Column<int>(type: "integer", nullable: false),
                    Url = table.Column<string>(type: "text", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_DownloadLink", x => x.Id);
                    table.ForeignKey(
                        name: "FK_DownloadLink_GamingPlatforms_GamingPlatformId",
                        column: x => x.GamingPlatformId,
                        principalTable: "GamingPlatforms",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_DownloadLink_VisualNovels_VisualNovelId",
                        column: x => x.VisualNovelId,
                        principalTable: "VisualNovels",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_DownloadLink_GamingPlatformId",
                table: "DownloadLink",
                column: "GamingPlatformId");

            migrationBuilder.CreateIndex(
                name: "IX_DownloadLink_VisualNovelId",
                table: "DownloadLink",
                column: "VisualNovelId");

            //migrationBuilder.AddForeignKey(
            //    name: "FK_VisualNovels_Authors_AuthorId",
            //    table: "VisualNovels",
            //    column: "AuthorId",
            //    principalTable: "Authors",
            //    principalColumn: "Id",
            //    onDelete: ReferentialAction.Cascade);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_VisualNovels_Authors_AuthorId",
                table: "VisualNovels");

            migrationBuilder.DropTable(
                name: "DownloadLink");

            migrationBuilder.DropColumn(
                name: "Status",
                table: "VisualNovels");

            migrationBuilder.DropColumn(
                name: "VndbId",
                table: "Authors");

            migrationBuilder.AlterColumn<int>(
                name: "AuthorId",
                table: "VisualNovels",
                type: "integer",
                nullable: true,
                oldClrType: typeof(int),
                oldType: "integer");

            migrationBuilder.AddColumn<string>(
                name: "Autor",
                table: "VisualNovels",
                type: "text",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "Translator",
                table: "VisualNovels",
                type: "text",
                nullable: true);

            //migrationBuilder.AddForeignKey(
            //    name: "FK_VisualNovels_Authors_AuthorId",
            //    table: "VisualNovels",
            //    column: "AuthorId",
            //    principalTable: "Authors",
            //    principalColumn: "Id");
        }
    }
}
