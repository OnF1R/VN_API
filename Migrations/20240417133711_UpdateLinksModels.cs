using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace VNAPI.Migrations
{
    /// <inheritdoc />
    public partial class UpdateLinksModels : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_DownloadLinks_VisualNovels_VisualNovelId",
                table: "DownloadLinks");

            migrationBuilder.DropForeignKey(
                name: "FK_OtherLinks_VisualNovels_VisualNovelId",
                table: "OtherLinks");

            migrationBuilder.AlterColumn<int>(
                name: "VisualNovelId",
                table: "OtherLinks",
                type: "integer",
                nullable: true,
                oldClrType: typeof(int),
                oldType: "integer");

            migrationBuilder.AlterColumn<int>(
                name: "VisualNovelId",
                table: "DownloadLinks",
                type: "integer",
                nullable: true,
                oldClrType: typeof(int),
                oldType: "integer");

            migrationBuilder.AddForeignKey(
                name: "FK_DownloadLinks_VisualNovels_VisualNovelId",
                table: "DownloadLinks",
                column: "VisualNovelId",
                principalTable: "VisualNovels",
                principalColumn: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_OtherLinks_VisualNovels_VisualNovelId",
                table: "OtherLinks",
                column: "VisualNovelId",
                principalTable: "VisualNovels",
                principalColumn: "Id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_DownloadLinks_VisualNovels_VisualNovelId",
                table: "DownloadLinks");

            migrationBuilder.DropForeignKey(
                name: "FK_OtherLinks_VisualNovels_VisualNovelId",
                table: "OtherLinks");

            migrationBuilder.AlterColumn<int>(
                name: "VisualNovelId",
                table: "OtherLinks",
                type: "integer",
                nullable: false,
                defaultValue: 0,
                oldClrType: typeof(int),
                oldType: "integer",
                oldNullable: true);

            migrationBuilder.AlterColumn<int>(
                name: "VisualNovelId",
                table: "DownloadLinks",
                type: "integer",
                nullable: false,
                defaultValue: 0,
                oldClrType: typeof(int),
                oldType: "integer",
                oldNullable: true);

            migrationBuilder.AddForeignKey(
                name: "FK_DownloadLinks_VisualNovels_VisualNovelId",
                table: "DownloadLinks",
                column: "VisualNovelId",
                principalTable: "VisualNovels",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_OtherLinks_VisualNovels_VisualNovelId",
                table: "OtherLinks",
                column: "VisualNovelId",
                principalTable: "VisualNovels",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }
    }
}
