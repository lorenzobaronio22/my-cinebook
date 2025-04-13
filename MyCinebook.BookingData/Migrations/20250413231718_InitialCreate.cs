using System;
using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace MyCinebook.BookingData.Migrations
{
    /// <inheritdoc />
    public partial class InitialCreate : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "BookingModel",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    DeletedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_BookingModel", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "BookingShowModel",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    Title = table.Column<string>(type: "text", nullable: false),
                    BookingModelId = table.Column<int>(type: "integer", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_BookingShowModel", x => x.Id);
                    table.ForeignKey(
                        name: "FK_BookingShowModel_BookingModel_BookingModelId",
                        column: x => x.BookingModelId,
                        principalTable: "BookingModel",
                        principalColumn: "Id");
                });

            migrationBuilder.CreateTable(
                name: "BookingSeatModel",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    Line = table.Column<char>(type: "character(1)", nullable: false),
                    Number = table.Column<int>(type: "integer", nullable: false),
                    BookingShowModelId = table.Column<int>(type: "integer", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_BookingSeatModel", x => x.Id);
                    table.ForeignKey(
                        name: "FK_BookingSeatModel_BookingShowModel_BookingShowModelId",
                        column: x => x.BookingShowModelId,
                        principalTable: "BookingShowModel",
                        principalColumn: "Id");
                });

            migrationBuilder.CreateIndex(
                name: "IX_BookingSeatModel_BookingShowModelId",
                table: "BookingSeatModel",
                column: "BookingShowModelId");

            migrationBuilder.CreateIndex(
                name: "IX_BookingShowModel_BookingModelId",
                table: "BookingShowModel",
                column: "BookingModelId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "BookingSeatModel");

            migrationBuilder.DropTable(
                name: "BookingShowModel");

            migrationBuilder.DropTable(
                name: "BookingModel");
        }
    }
}
