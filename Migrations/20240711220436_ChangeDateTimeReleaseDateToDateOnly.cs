using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace VNAPI.Migrations
{
    /// <inheritdoc />
    public partial class ChangeDateTimeReleaseDateToDateOnly : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterColumn<DateOnly>(
                name: "ReleaseDate",
                table: "VisualNovels",
                type: "date",
                nullable: true,
                computedColumnSql: "CASE WHEN \"ReleaseYear\" IS NOT NULL THEN MAKE_DATE(\"ReleaseYear\", COALESCE(\"ReleaseMonth\", 1), COALESCE(\"ReleaseDay\", 1)) ELSE NULL END",
                stored: true,
                oldClrType: typeof(DateTime),
                oldType: "timestamp without time zone",
                oldNullable: true,
                oldComputedColumnSql: "CASE WHEN \"ReleaseYear\" IS NOT NULL THEN MAKE_DATE(\"ReleaseYear\", COALESCE(\"ReleaseMonth\", 1), COALESCE(\"ReleaseDay\", 1)) ELSE NULL END",
                oldStored: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterColumn<DateTime>(
                name: "ReleaseDate",
                table: "VisualNovels",
                type: "timestamp without time zone",
                nullable: true,
                computedColumnSql: "CASE WHEN \"ReleaseYear\" IS NOT NULL THEN MAKE_DATE(\"ReleaseYear\", COALESCE(\"ReleaseMonth\", 1), COALESCE(\"ReleaseDay\", 1)) ELSE NULL END",
                stored: true,
                oldClrType: typeof(DateOnly),
                oldType: "date",
                oldNullable: true,
                oldComputedColumnSql: "CASE WHEN \"ReleaseYear\" IS NOT NULL THEN MAKE_DATE(\"ReleaseYear\", COALESCE(\"ReleaseMonth\", 1), COALESCE(\"ReleaseDay\", 1)) ELSE NULL END",
                oldStored: true);
        }
    }
}
