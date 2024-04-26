using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace VNAPI.Migrations
{
    /// <inheritdoc />
    public partial class UpdateTagTable : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterColumn<string>(
                name: "Description",
                table: "Tags",
                type: "text",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "text");

            migrationBuilder.AddColumn<bool>(
                name: "Applicable",
                table: "Tags",
                type: "boolean",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "Category",
                table: "Tags",
                type: "integer",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "EnglishName",
                table: "Tags",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "VndbId",
                table: "Tags",
                type: "text",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Applicable",
                table: "Tags");

            migrationBuilder.DropColumn(
                name: "Category",
                table: "Tags");

            migrationBuilder.DropColumn(
                name: "EnglishName",
                table: "Tags");

            migrationBuilder.DropColumn(
                name: "VndbId",
                table: "Tags");

            migrationBuilder.AlterColumn<string>(
                name: "Description",
                table: "Tags",
                type: "text",
                nullable: false,
                defaultValue: "",
                oldClrType: typeof(string),
                oldType: "text",
                oldNullable: true);
        }
    }
}
