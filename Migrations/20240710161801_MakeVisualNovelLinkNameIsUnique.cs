using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace VNAPI.Migrations
{
    /// <inheritdoc />
    public partial class MakeVisualNovelLinkNameIsUnique : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_VisualNovels_LinkName",
                table: "VisualNovels");

            migrationBuilder.CreateIndex(
                name: "IX_VisualNovels_LinkName",
                table: "VisualNovels",
                column: "LinkName",
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_VisualNovels_LinkName",
                table: "VisualNovels");

            migrationBuilder.CreateIndex(
                name: "IX_VisualNovels_LinkName",
                table: "VisualNovels",
                column: "LinkName");
        }
    }
}
