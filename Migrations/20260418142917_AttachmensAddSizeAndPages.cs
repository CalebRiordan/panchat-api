using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace PanChatApi.Migrations
{
    /// <inheritdoc />
    public partial class AttachmensAddSizeAndPages : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "page_count",
                table: "attachments",
                type: "integer",
                nullable: true);

            migrationBuilder.AddColumn<long>(
                name: "size",
                table: "attachments",
                type: "bigint",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "page_count",
                table: "attachments");

            migrationBuilder.DropColumn(
                name: "size",
                table: "attachments");
        }
    }
}
