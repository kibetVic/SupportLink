using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace SupportLink.Migrations
{
    /// <inheritdoc />
    public partial class SupperLinkss : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "UpdateDetails",
                table: "TicketUpdates");

            migrationBuilder.AddColumn<int>(
                name: "Status",
                table: "TicketUpdates",
                type: "int",
                nullable: false,
                defaultValue: 0);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Status",
                table: "TicketUpdates");

            migrationBuilder.AddColumn<string>(
                name: "UpdateDetails",
                table: "TicketUpdates",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");
        }
    }
}
