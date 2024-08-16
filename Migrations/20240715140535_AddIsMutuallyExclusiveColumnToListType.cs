using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace VNAPI.Migrations
{
    /// <inheritdoc />
    public partial class AddIsMutuallyExclusiveColumnToListType : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterColumn<DateOnly>(
                name: "ReleaseDate",
                table: "VisualNovels",
                type: "date",
                nullable: true,
                oldClrType: typeof(DateOnly),
                oldType: "date",
                oldNullable: true,
                oldComputedColumnSql: "CASE WHEN \"ReleaseYear\" IS NOT NULL THEN MAKE_DATE(\"ReleaseYear\", COALESCE(\"ReleaseMonth\", 1), COALESCE(\"ReleaseDay\", 1)) ELSE NULL END");

            migrationBuilder.AddColumn<bool>(
                name: "IsMutuallyExclusive",
                table: "ListTypes",
                type: "boolean",
                nullable: false,
                defaultValue: false);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "IsMutuallyExclusive",
                table: "ListTypes");

            migrationBuilder.AlterColumn<DateOnly>(
                name: "ReleaseDate",
                table: "VisualNovels",
                type: "date",
                nullable: true,
                computedColumnSql: "CASE WHEN \"ReleaseYear\" IS NOT NULL THEN MAKE_DATE(\"ReleaseYear\", COALESCE(\"ReleaseMonth\", 1), COALESCE(\"ReleaseDay\", 1)) ELSE NULL END",
                stored: true,
                oldClrType: typeof(DateOnly),
                oldType: "date",
                oldNullable: true);
        }
    }
}
