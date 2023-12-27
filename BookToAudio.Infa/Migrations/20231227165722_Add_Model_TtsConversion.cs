using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace BookToAudio.Infa.Migrations
{
    /// <inheritdoc />
    public partial class Add_Model_TtsConversion : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "TtsConversions",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    Status = table.Column<int>(type: "integer", nullable: false),
                    Model = table.Column<string>(type: "text", nullable: false),
                    Voice = table.Column<int>(type: "integer", nullable: false),
                    ResponseFormat = table.Column<string>(type: "text", nullable: false),
                    Speed = table.Column<double>(type: "double precision", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_TtsConversions", x => x.Id);
                });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "TtsConversions");
        }
    }
}
