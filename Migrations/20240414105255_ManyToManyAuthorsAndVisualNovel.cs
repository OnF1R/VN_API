using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace VNAPI.Migrations
{
    /// <inheritdoc />
    public partial class ManyToManyAuthorsAndVisualNovel : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            //migrationBuilder.DropForeignKey(
            //    name: "FK_VisualNovels_Authors_AuthorId",
            //    table: "VisualNovels");

            migrationBuilder.DropIndex(
                name: "IX_VisualNovels_AuthorId",
                table: "VisualNovels");

            migrationBuilder.DropColumn(
                name: "AuthorId",
                table: "VisualNovels");

            migrationBuilder.CreateTable(
                name: "AuthorVisualNovel",
                columns: table => new
                {
                    AuthorId = table.Column<int>(type: "integer", nullable: false),
                    VisualNovelsId = table.Column<int>(type: "integer", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AuthorVisualNovel", x => new { x.AuthorId, x.VisualNovelsId });
                    table.ForeignKey(
                        name: "FK_AuthorVisualNovel_Authors_AuthorId",
                        column: x => x.AuthorId,
                        principalTable: "Authors",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_AuthorVisualNovel_VisualNovels_VisualNovelsId",
                        column: x => x.VisualNovelsId,
                        principalTable: "VisualNovels",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_AuthorVisualNovel_VisualNovelsId",
                table: "AuthorVisualNovel",
                column: "VisualNovelsId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "AuthorVisualNovel");

            migrationBuilder.AddColumn<int>(
                name: "AuthorId",
                table: "VisualNovels",
                type: "integer",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.CreateIndex(
                name: "IX_VisualNovels_AuthorId",
                table: "VisualNovels",
                column: "AuthorId");

            migrationBuilder.AddForeignKey(
                name: "FK_VisualNovels_Authors_AuthorId",
                table: "VisualNovels",
                column: "AuthorId",
                principalTable: "Authors",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }
    }
}
