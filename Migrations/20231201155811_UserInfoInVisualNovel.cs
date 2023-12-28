using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace VNAPI.Migrations
{
    /// <inheritdoc />
    public partial class UserInfoInVisualNovel : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<Guid>(
                name: "AdddeUserId",
                table: "VisualNovels",
                type: "TEXT",
                nullable: false,
                defaultValue: new Guid("00000000-0000-0000-0000-000000000000"));

            migrationBuilder.AddColumn<Guid>(
                name: "AuthorId",
                table: "VisualNovels",
                type: "TEXT",
                nullable: true);

            migrationBuilder.AddColumn<Guid>(
                name: "TranslatorId",
                table: "VisualNovels",
                type: "TEXT",
                nullable: true);

            migrationBuilder.CreateTable(
                name: "Authors",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "TEXT", nullable: false),
                    Name = table.Column<string>(type: "TEXT", nullable: false),
                    Source = table.Column<string>(type: "TEXT", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Authors", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Translators",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "TEXT", nullable: false),
                    Name = table.Column<string>(type: "TEXT", nullable: false),
                    Source = table.Column<string>(type: "TEXT", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Translators", x => x.Id);
                });

            migrationBuilder.CreateIndex(
                name: "IX_VisualNovels_AuthorId",
                table: "VisualNovels",
                column: "AuthorId");

            migrationBuilder.CreateIndex(
                name: "IX_VisualNovels_TranslatorId",
                table: "VisualNovels",
                column: "TranslatorId");

            migrationBuilder.AddForeignKey(
                name: "FK_VisualNovels_Authors_AuthorId",
                table: "VisualNovels",
                column: "AuthorId",
                principalTable: "Authors",
                principalColumn: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_VisualNovels_Translators_TranslatorId",
                table: "VisualNovels",
                column: "TranslatorId",
                principalTable: "Translators",
                principalColumn: "Id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_VisualNovels_Authors_AuthorId",
                table: "VisualNovels");

            migrationBuilder.DropForeignKey(
                name: "FK_VisualNovels_Translators_TranslatorId",
                table: "VisualNovels");

            migrationBuilder.DropTable(
                name: "Authors");

            migrationBuilder.DropTable(
                name: "Translators");

            migrationBuilder.DropIndex(
                name: "IX_VisualNovels_AuthorId",
                table: "VisualNovels");

            migrationBuilder.DropIndex(
                name: "IX_VisualNovels_TranslatorId",
                table: "VisualNovels");

            migrationBuilder.DropColumn(
                name: "AdddeUserId",
                table: "VisualNovels");

            migrationBuilder.DropColumn(
                name: "AuthorId",
                table: "VisualNovels");

            migrationBuilder.DropColumn(
                name: "TranslatorId",
                table: "VisualNovels");
        }
    }
}
