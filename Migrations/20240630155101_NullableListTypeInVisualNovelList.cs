using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace VNAPI.Migrations
{
    /// <inheritdoc />
    public partial class NullableListTypeInVisualNovelList : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_VisualNovelLists_ListTypes_ListTypeId",
                table: "VisualNovelLists");

            migrationBuilder.AlterColumn<int>(
                name: "ListTypeId",
                table: "VisualNovelLists",
                type: "integer",
                nullable: true,
                oldClrType: typeof(int),
                oldType: "integer");

            migrationBuilder.AddForeignKey(
                name: "FK_VisualNovelLists_ListTypes_ListTypeId",
                table: "VisualNovelLists",
                column: "ListTypeId",
                principalTable: "ListTypes",
                principalColumn: "Id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_VisualNovelLists_ListTypes_ListTypeId",
                table: "VisualNovelLists");

            migrationBuilder.AlterColumn<int>(
                name: "ListTypeId",
                table: "VisualNovelLists",
                type: "integer",
                nullable: false,
                defaultValue: 0,
                oldClrType: typeof(int),
                oldType: "integer",
                oldNullable: true);

            migrationBuilder.AddForeignKey(
                name: "FK_VisualNovelLists_ListTypes_ListTypeId",
                table: "VisualNovelLists",
                column: "ListTypeId",
                principalTable: "ListTypes",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }
    }
}
