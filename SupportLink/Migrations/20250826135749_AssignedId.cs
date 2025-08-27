using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace SupportLink.Migrations
{
    /// <inheritdoc />
    public partial class AssignedId : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_SupportTickets_Users_AssignedAgentId",
                table: "SupportTickets");

            migrationBuilder.RenameColumn(
                name: "AssignedAgentId",
                table: "SupportTickets",
                newName: "AssignedId");

            migrationBuilder.RenameIndex(
                name: "IX_SupportTickets_AssignedAgentId",
                table: "SupportTickets",
                newName: "IX_SupportTickets_AssignedId");

            migrationBuilder.AddForeignKey(
                name: "FK_SupportTickets_Users_AssignedId",
                table: "SupportTickets",
                column: "AssignedId",
                principalTable: "Users",
                principalColumn: "UserId",
                onDelete: ReferentialAction.Restrict);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_SupportTickets_Users_AssignedId",
                table: "SupportTickets");

            migrationBuilder.RenameColumn(
                name: "AssignedId",
                table: "SupportTickets",
                newName: "AssignedAgentId");

            migrationBuilder.RenameIndex(
                name: "IX_SupportTickets_AssignedId",
                table: "SupportTickets",
                newName: "IX_SupportTickets_AssignedAgentId");

            migrationBuilder.AddForeignKey(
                name: "FK_SupportTickets_Users_AssignedAgentId",
                table: "SupportTickets",
                column: "AssignedAgentId",
                principalTable: "Users",
                principalColumn: "UserId",
                onDelete: ReferentialAction.Restrict);
        }
    }
}
