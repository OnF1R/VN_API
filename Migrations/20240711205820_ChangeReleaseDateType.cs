using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace VNAPI.Migrations
{
    /// <inheritdoc />
    public partial class ChangeReleaseDateType : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "ReleaseYear",
                table: "VisualNovels");

            migrationBuilder.AddColumn<DateOnly>(
                name: "ReleaseDate",
                table: "VisualNovels",
                type: "date",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "ReleaseDate",
                table: "VisualNovels");

            migrationBuilder.AddColumn<int>(
                name: "ReleaseYear",
                table: "VisualNovels",
                type: "integer",
                nullable: true);
        }
    }
}
