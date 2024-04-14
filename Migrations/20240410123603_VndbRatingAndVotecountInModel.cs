using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace VNAPI.Migrations
{
    /// <inheritdoc />
    public partial class VndbRatingAndVotecountInModel : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<double>(
                name: "VndbRating",
                table: "VisualNovels",
                type: "double precision",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "VndbVoteCount",
                table: "VisualNovels",
                type: "integer",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "VndbRating",
                table: "VisualNovels");

            migrationBuilder.DropColumn(
                name: "VndbVoteCount",
                table: "VisualNovels");
        }
    }
}
