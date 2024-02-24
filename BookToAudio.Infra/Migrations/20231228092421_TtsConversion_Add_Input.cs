using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace BookToAudio.Infra.Migrations
{
    /// <inheritdoc />
    public partial class TtsConversion_Add_Input : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "Input",
                table: "TtsConversions",
                type: "text",
                nullable: false,
                defaultValue: "");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Input",
                table: "TtsConversions");
        }
    }
}
