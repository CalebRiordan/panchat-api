using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace PanChatApi.Migrations
{
    /// <inheritdoc />
    public partial class RemoveAttachmentType : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "type",
                table: "attachments");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "type",
                table: "attachments",
                type: "integer",
                maxLength: 5,
                nullable: false,
                defaultValue: 0);
        }
    }
}
