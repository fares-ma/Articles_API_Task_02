using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Persistence.Migrations
{
    /// <inheritdoc />
    public partial class AddNewspapersTable : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "NewspaperId",
                table: "Articles",
                type: "int",
                nullable: true);

            migrationBuilder.CreateTable(
                name: "Newspapers",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Name = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: false),
                    Description = table.Column<string>(type: "nvarchar(1000)", maxLength: 1000, nullable: false),
                    Publisher = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: false),
                    Website = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: false),
                    LogoUrl = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: false),
                    FoundedDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    IsActive = table.Column<bool>(type: "bit", nullable: false, defaultValue: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Newspapers", x => x.Id);
                });

            migrationBuilder.CreateIndex(
                name: "IX_Articles_NewspaperId",
                table: "Articles",
                column: "NewspaperId");

            migrationBuilder.CreateIndex(
                name: "IX_Newspapers_IsActive",
                table: "Newspapers",
                column: "IsActive");

            migrationBuilder.CreateIndex(
                name: "IX_Newspapers_Name",
                table: "Newspapers",
                column: "Name",
                unique: true);

            migrationBuilder.AddForeignKey(
                name: "FK_Articles_Newspapers_NewspaperId",
                table: "Articles",
                column: "NewspaperId",
                principalTable: "Newspapers",
                principalColumn: "Id",
                onDelete: ReferentialAction.SetNull);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Articles_Newspapers_NewspaperId",
                table: "Articles");

            migrationBuilder.DropTable(
                name: "Newspapers");

            migrationBuilder.DropIndex(
                name: "IX_Articles_NewspaperId",
                table: "Articles");

            migrationBuilder.DropColumn(
                name: "NewspaperId",
                table: "Articles");
        }
    }
}
