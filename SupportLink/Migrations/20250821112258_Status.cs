using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace SupportLink.Migrations
{
    /// <inheritdoc />
    public partial class Status : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.UpdateData(
                table: "TicketStatuses",
                keyColumn: "StatusId",
                keyValue: 4,
                columns: new[] { "Description", "Name" },
                values: new object[] { "Issue Solved", "Solved" });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.UpdateData(
                table: "TicketStatuses",
                keyColumn: "StatusId",
                keyValue: 4,
                columns: new[] { "Description", "Name" },
                values: new object[] { "Issue resolved", "Resolved" });
        }
    }
}
