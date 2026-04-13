using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace PanChatApi.Migrations
{
    /// <inheritdoc />
    public partial class AttachmentAddType : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "type",
                table: "attachments",
                type: "text",
                nullable: false,
                defaultValue: "");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "type",
                table: "attachments");
        }
    }
}
