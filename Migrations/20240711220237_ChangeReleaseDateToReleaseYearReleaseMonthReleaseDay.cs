using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace VNAPI.Migrations
{
    /// <inheritdoc />
    public partial class ChangeReleaseDateToReleaseYearReleaseMonthReleaseDay : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "ReleaseDay",
                table: "VisualNovels",
                type: "integer",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "ReleaseMonth",
                table: "VisualNovels",
                type: "integer",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "ReleaseYear",
                table: "VisualNovels",
                type: "integer",
                nullable: true);

            migrationBuilder.AlterColumn<DateTime>(
                name: "ReleaseDate",
                table: "VisualNovels",
                type: "timestamp without time zone",
                nullable: true,
                computedColumnSql: "CASE WHEN \"ReleaseYear\" IS NOT NULL THEN MAKE_DATE(\"ReleaseYear\", COALESCE(\"ReleaseMonth\", 1), COALESCE(\"ReleaseDay\", 1)) ELSE NULL END",
                stored: true,
                oldClrType: typeof(DateOnly),
                oldType: "date",
                oldNullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "ReleaseDay",
                table: "VisualNovels");

            migrationBuilder.DropColumn(
                name: "ReleaseMonth",
                table: "VisualNovels");

            migrationBuilder.DropColumn(
                name: "ReleaseYear",
                table: "VisualNovels");

            migrationBuilder.AlterColumn<DateOnly>(
                name: "ReleaseDate",
                table: "VisualNovels",
                type: "date",
                nullable: true,
                oldClrType: typeof(DateTime),
                oldType: "timestamp without time zone",
                oldNullable: true,
                oldComputedColumnSql: "CASE WHEN \"ReleaseYear\" IS NOT NULL THEN MAKE_DATE(\"ReleaseYear\", COALESCE(\"ReleaseMonth\", 1), COALESCE(\"ReleaseDay\", 1)) ELSE NULL END");
        }
    }
}
