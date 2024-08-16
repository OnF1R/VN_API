using System.Collections.Generic;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace VNAPI.Migrations
{
    /// <inheritdoc />
    public partial class UpdateVisualNovelModelAddRelatedNovelsAnotherTitlesLinkName : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_VisualNovels_Translators_TranslatorId",
                table: "VisualNovels");

            migrationBuilder.DropIndex(
                name: "IX_VisualNovels_OriginalTitle",
                table: "VisualNovels");

            migrationBuilder.DropColumn(
                name: "OriginalTitle",
                table: "VisualNovels");

            migrationBuilder.RenameColumn(
                name: "TranslatorId",
                table: "VisualNovels",
                newName: "VisualNovelId");

            migrationBuilder.RenameIndex(
                name: "IX_VisualNovels_TranslatorId",
                table: "VisualNovels",
                newName: "IX_VisualNovels_VisualNovelId");

            migrationBuilder.AlterColumn<List<string>>(
                name: "ScreenshotLinks",
                table: "VisualNovels",
                type: "text[]",
                nullable: true,
                oldClrType: typeof(List<string>),
                oldType: "text[]");

            migrationBuilder.AddColumn<List<string>>(
                name: "AnotherTitles",
                table: "VisualNovels",
                type: "text[]",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "LinkName",
                table: "VisualNovels",
                type: "text",
                nullable: false,
                defaultValue: "");

            migrationBuilder.CreateTable(
                name: "TranslatorVisualNovel",
                columns: table => new
                {
                    TranslatorId = table.Column<int>(type: "integer", nullable: false),
                    VisualNovelsId = table.Column<int>(type: "integer", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_TranslatorVisualNovel", x => new { x.TranslatorId, x.VisualNovelsId });
                    table.ForeignKey(
                        name: "FK_TranslatorVisualNovel_Translators_TranslatorId",
                        column: x => x.TranslatorId,
                        principalTable: "Translators",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_TranslatorVisualNovel_VisualNovels_VisualNovelsId",
                        column: x => x.VisualNovelsId,
                        principalTable: "VisualNovels",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_VisualNovels_LinkName",
                table: "VisualNovels",
                column: "LinkName");

            migrationBuilder.CreateIndex(
                name: "IX_TranslatorVisualNovel_VisualNovelsId",
                table: "TranslatorVisualNovel",
                column: "VisualNovelsId");

            migrationBuilder.AddForeignKey(
                name: "FK_VisualNovels_VisualNovels_VisualNovelId",
                table: "VisualNovels",
                column: "VisualNovelId",
                principalTable: "VisualNovels",
                principalColumn: "Id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_VisualNovels_VisualNovels_VisualNovelId",
                table: "VisualNovels");

            migrationBuilder.DropTable(
                name: "TranslatorVisualNovel");

            migrationBuilder.DropIndex(
                name: "IX_VisualNovels_LinkName",
                table: "VisualNovels");

            migrationBuilder.DropColumn(
                name: "AnotherTitles",
                table: "VisualNovels");

            migrationBuilder.DropColumn(
                name: "LinkName",
                table: "VisualNovels");

            migrationBuilder.RenameColumn(
                name: "VisualNovelId",
                table: "VisualNovels",
                newName: "TranslatorId");

            migrationBuilder.RenameIndex(
                name: "IX_VisualNovels_VisualNovelId",
                table: "VisualNovels",
                newName: "IX_VisualNovels_TranslatorId");

            migrationBuilder.AlterColumn<List<string>>(
                name: "ScreenshotLinks",
                table: "VisualNovels",
                type: "text[]",
                nullable: false,
                oldClrType: typeof(List<string>),
                oldType: "text[]",
                oldNullable: true);

            migrationBuilder.AddColumn<string>(
                name: "OriginalTitle",
                table: "VisualNovels",
                type: "text",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_VisualNovels_OriginalTitle",
                table: "VisualNovels",
                column: "OriginalTitle");

            migrationBuilder.AddForeignKey(
                name: "FK_VisualNovels_Translators_TranslatorId",
                table: "VisualNovels",
                column: "TranslatorId",
                principalTable: "Translators",
                principalColumn: "Id");
        }
    }
}
