using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace SupportLink.Migrations
{
    /// <inheritdoc />
    public partial class AddFileColumnsToSupportTicket : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "FileType",
                table: "SupportTickets",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "UploadFile",
                table: "SupportTickets",
                type: "nvarchar(max)",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "FileType",
                table: "SupportTickets");

            migrationBuilder.DropColumn(
                name: "UploadFile",
                table: "SupportTickets");
        }
    }
}
