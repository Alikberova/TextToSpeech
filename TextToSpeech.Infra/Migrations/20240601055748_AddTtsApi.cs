using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace TextToSpeech.Infra.Migrations
{
    /// <inheritdoc />
    public partial class AddTtsApi : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<Guid>(
                name: "TtsApiId",
                table: "AudioFiles",
                type: "uuid",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "Type",
                table: "AudioFiles",
                type: "integer",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.CreateTable(
                name: "TtsApis",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    Name = table.Column<string>(type: "text", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_TtsApis", x => x.Id);
                });

            migrationBuilder.CreateIndex(
                name: "IX_AudioFiles_TtsApiId",
                table: "AudioFiles",
                column: "TtsApiId");

            migrationBuilder.AddForeignKey(
                name: "FK_AudioFiles_TtsApis_TtsApiId",
                table: "AudioFiles",
                column: "TtsApiId",
                principalTable: "TtsApis",
                principalColumn: "Id",
                onDelete: ReferentialAction.SetNull);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_AudioFiles_TtsApis_TtsApiId",
                table: "AudioFiles");

            migrationBuilder.DropTable(
                name: "TtsApis");

            migrationBuilder.DropIndex(
                name: "IX_AudioFiles_TtsApiId",
                table: "AudioFiles");

            migrationBuilder.DropColumn(
                name: "TtsApiId",
                table: "AudioFiles");

            migrationBuilder.DropColumn(
                name: "Type",
                table: "AudioFiles");
        }
    }
}
