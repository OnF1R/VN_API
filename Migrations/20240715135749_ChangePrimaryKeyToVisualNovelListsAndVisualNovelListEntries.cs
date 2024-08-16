using System;
using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace VNAPI.Migrations
{
    /// <inheritdoc />
    public partial class ChangePrimaryKeyToVisualNovelListsAndVisualNovelListEntries : Migration
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

            migrationBuilder.AlterColumn<decimal>(
                name: "Id",
                table: "VisualNovelLists",
                type: "numeric(20,0)",
                nullable: false,
                oldClrType: typeof(int),
                oldType: "integer")
                .OldAnnotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn);

            migrationBuilder.AlterColumn<decimal>(
                name: "VisualNovelListId",
                table: "VisualNovelListEntries",
                type: "numeric(20,0)",
                nullable: false,
                oldClrType: typeof(int),
                oldType: "integer");

            migrationBuilder.AlterColumn<decimal>(
                name: "Id",
                table: "VisualNovelListEntries",
                type: "numeric(20,0)",
                nullable: false,
                oldClrType: typeof(int),
                oldType: "integer")
                .OldAnnotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn);

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

            migrationBuilder.AlterColumn<int>(
                name: "Id",
                table: "VisualNovelLists",
                type: "integer",
                nullable: false,
                oldClrType: typeof(decimal),
                oldType: "numeric(20,0)")
                .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn);

            migrationBuilder.AlterColumn<int>(
                name: "VisualNovelListId",
                table: "VisualNovelListEntries",
                type: "integer",
                nullable: false,
                oldClrType: typeof(decimal),
                oldType: "numeric(20,0)");

            migrationBuilder.AlterColumn<int>(
                name: "Id",
                table: "VisualNovelListEntries",
                type: "integer",
                nullable: false,
                oldClrType: typeof(decimal),
                oldType: "numeric(20,0)")
                .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn);

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
