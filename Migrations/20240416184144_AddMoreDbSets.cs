using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace VNAPI.Migrations
{
    /// <inheritdoc />
    public partial class AddMoreDbSets : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_DownloadLink_GamingPlatforms_GamingPlatformId",
                table: "DownloadLink");

            migrationBuilder.DropForeignKey(
                name: "FK_DownloadLink_VisualNovels_VisualNovelId",
                table: "DownloadLink");

            migrationBuilder.DropForeignKey(
                name: "FK_OtherLink_VisualNovels_VisualNovelId",
                table: "OtherLink");

            migrationBuilder.DropPrimaryKey(
                name: "PK_OtherLink",
                table: "OtherLink");

            migrationBuilder.DropPrimaryKey(
                name: "PK_DownloadLink",
                table: "DownloadLink");

            migrationBuilder.RenameTable(
                name: "OtherLink",
                newName: "OtherLinks");

            migrationBuilder.RenameTable(
                name: "DownloadLink",
                newName: "DownloadLinks");

            migrationBuilder.RenameIndex(
                name: "IX_OtherLink_VisualNovelId",
                table: "OtherLinks",
                newName: "IX_OtherLinks_VisualNovelId");

            migrationBuilder.RenameIndex(
                name: "IX_DownloadLink_VisualNovelId",
                table: "DownloadLinks",
                newName: "IX_DownloadLinks_VisualNovelId");

            migrationBuilder.RenameIndex(
                name: "IX_DownloadLink_GamingPlatformId",
                table: "DownloadLinks",
                newName: "IX_DownloadLinks_GamingPlatformId");

            migrationBuilder.AddPrimaryKey(
                name: "PK_OtherLinks",
                table: "OtherLinks",
                column: "Id");

            migrationBuilder.AddPrimaryKey(
                name: "PK_DownloadLinks",
                table: "DownloadLinks",
                column: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_DownloadLinks_GamingPlatforms_GamingPlatformId",
                table: "DownloadLinks",
                column: "GamingPlatformId",
                principalTable: "GamingPlatforms",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

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

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_DownloadLinks_GamingPlatforms_GamingPlatformId",
                table: "DownloadLinks");

            migrationBuilder.DropForeignKey(
                name: "FK_DownloadLinks_VisualNovels_VisualNovelId",
                table: "DownloadLinks");

            migrationBuilder.DropForeignKey(
                name: "FK_OtherLinks_VisualNovels_VisualNovelId",
                table: "OtherLinks");

            migrationBuilder.DropPrimaryKey(
                name: "PK_OtherLinks",
                table: "OtherLinks");

            migrationBuilder.DropPrimaryKey(
                name: "PK_DownloadLinks",
                table: "DownloadLinks");

            migrationBuilder.RenameTable(
                name: "OtherLinks",
                newName: "OtherLink");

            migrationBuilder.RenameTable(
                name: "DownloadLinks",
                newName: "DownloadLink");

            migrationBuilder.RenameIndex(
                name: "IX_OtherLinks_VisualNovelId",
                table: "OtherLink",
                newName: "IX_OtherLink_VisualNovelId");

            migrationBuilder.RenameIndex(
                name: "IX_DownloadLinks_VisualNovelId",
                table: "DownloadLink",
                newName: "IX_DownloadLink_VisualNovelId");

            migrationBuilder.RenameIndex(
                name: "IX_DownloadLinks_GamingPlatformId",
                table: "DownloadLink",
                newName: "IX_DownloadLink_GamingPlatformId");

            migrationBuilder.AddPrimaryKey(
                name: "PK_OtherLink",
                table: "OtherLink",
                column: "Id");

            migrationBuilder.AddPrimaryKey(
                name: "PK_DownloadLink",
                table: "DownloadLink",
                column: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_DownloadLink_GamingPlatforms_GamingPlatformId",
                table: "DownloadLink",
                column: "GamingPlatformId",
                principalTable: "GamingPlatforms",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_DownloadLink_VisualNovels_VisualNovelId",
                table: "DownloadLink",
                column: "VisualNovelId",
                principalTable: "VisualNovels",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_OtherLink_VisualNovels_VisualNovelId",
                table: "OtherLink",
                column: "VisualNovelId",
                principalTable: "VisualNovels",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }
    }
}
