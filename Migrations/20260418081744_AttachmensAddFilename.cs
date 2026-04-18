using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace PanChatApi.Migrations
{
    /// <inheritdoc />
    public partial class AttachmensAddFilename : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "filename",
                table: "attachments",
                type: "text",
                nullable: false,
                defaultValue: "");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "filename",
                table: "attachments");
        }
    }
}
