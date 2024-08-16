using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace VNAPI.Migrations
{
    /// <inheritdoc />
    public partial class RemoveRelatedNovels : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_VisualNovels_VisualNovels_VisualNovelId",
                table: "VisualNovels");

            migrationBuilder.DropIndex(
                name: "IX_VisualNovels_VisualNovelId",
                table: "VisualNovels");

            migrationBuilder.DropColumn(
                name: "VisualNovelId",
                table: "VisualNovels");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "VisualNovelId",
                table: "VisualNovels",
                type: "integer",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_VisualNovels_VisualNovelId",
                table: "VisualNovels",
                column: "VisualNovelId");

            migrationBuilder.AddForeignKey(
                name: "FK_VisualNovels_VisualNovels_VisualNovelId",
                table: "VisualNovels",
                column: "VisualNovelId",
                principalTable: "VisualNovels",
                principalColumn: "Id");
        }
    }
}
