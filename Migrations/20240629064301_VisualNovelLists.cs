using System;
using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace VNAPI.Migrations
{
    /// <inheritdoc />
    public partial class VisualNovelLists : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "ListTypes",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    Name = table.Column<string>(type: "text", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ListTypes", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "VisualNovelLists",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    Name = table.Column<string>(type: "text", nullable: false),
                    IsCustom = table.Column<bool>(type: "boolean", nullable: false),
                    IsPrivate = table.Column<bool>(type: "boolean", nullable: false),
                    UserId = table.Column<string>(type: "text", nullable: false),
                    ListTypeId = table.Column<int>(type: "integer", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_VisualNovelLists", x => x.Id);
                    table.ForeignKey(
                        name: "FK_VisualNovelLists_ListTypes_ListTypeId",
                        column: x => x.ListTypeId,
                        principalTable: "ListTypes",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "VisualNovelListEntries",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    VisualNovelListId = table.Column<int>(type: "integer", nullable: false),
                    VisualNovelId = table.Column<int>(type: "integer", nullable: false),
                    AddingTime = table.Column<DateTime>(type: "timestamp without time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_VisualNovelListEntries", x => x.Id);
                    table.ForeignKey(
                        name: "FK_VisualNovelListEntries_VisualNovelLists_VisualNovelListId",
                        column: x => x.VisualNovelListId,
                        principalTable: "VisualNovelLists",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_VisualNovelListEntries_VisualNovels_VisualNovelId",
                        column: x => x.VisualNovelId,
                        principalTable: "VisualNovels",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_VisualNovelListEntries_VisualNovelId",
                table: "VisualNovelListEntries",
                column: "VisualNovelId");

            migrationBuilder.CreateIndex(
                name: "IX_VisualNovelListEntries_VisualNovelListId",
                table: "VisualNovelListEntries",
                column: "VisualNovelListId");

            migrationBuilder.CreateIndex(
                name: "IX_VisualNovelLists_ListTypeId",
                table: "VisualNovelLists",
                column: "ListTypeId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "VisualNovelListEntries");

            migrationBuilder.DropTable(
                name: "VisualNovelLists");

            migrationBuilder.DropTable(
                name: "ListTypes");
        }
    }
}
