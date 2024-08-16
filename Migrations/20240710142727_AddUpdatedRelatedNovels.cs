using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace VNAPI.Migrations
{
    /// <inheritdoc />
    public partial class AddUpdatedRelatedNovels : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "RelatedNovels",
                columns: table => new
                {
                    VisualNovelId = table.Column<int>(type: "integer", nullable: false),
                    RelatedVisualNovelId = table.Column<int>(type: "integer", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_RelatedNovels", x => new { x.VisualNovelId, x.RelatedVisualNovelId });
                    table.ForeignKey(
                        name: "FK_RelatedNovels_VisualNovels_RelatedVisualNovelId",
                        column: x => x.RelatedVisualNovelId,
                        principalTable: "VisualNovels",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_RelatedNovels_VisualNovels_VisualNovelId",
                        column: x => x.VisualNovelId,
                        principalTable: "VisualNovels",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(
                name: "IX_RelatedNovels_RelatedVisualNovelId",
                table: "RelatedNovels",
                column: "RelatedVisualNovelId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "RelatedNovels");
        }
    }
}
