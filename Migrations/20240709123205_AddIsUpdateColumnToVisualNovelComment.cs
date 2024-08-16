using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace VNAPI.Migrations
{
    /// <inheritdoc />
    public partial class AddIsUpdateColumnToVisualNovelComment : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<bool>(
                name: "IsUpdated",
                table: "Comments",
                type: "boolean",
                nullable: false,
                defaultValue: false);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "IsUpdated",
                table: "Comments");
        }
    }
}
