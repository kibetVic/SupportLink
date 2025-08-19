using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

#pragma warning disable CA1814 // Prefer jagged arrays over multidimensional

namespace SupportLink.Migrations
{
    /// <inheritdoc />
    public partial class AddFileColumnsToSupport : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "IssueCategory",
                table: "SupportTickets");

            migrationBuilder.DropColumn(
                name: "Status",
                table: "SupportTickets");

            migrationBuilder.AlterColumn<string>(
                name: "Description",
                table: "SupportTickets",
                type: "nvarchar(2000)",
                maxLength: 2000,
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(max)");

            migrationBuilder.AddColumn<int>(
                name: "CategoryId",
                table: "SupportTickets",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<int>(
                name: "StatusId",
                table: "SupportTickets",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.CreateTable(
                name: "IssueCategories",
                columns: table => new
                {
                    CategoryId = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Name = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    Description = table.Column<string>(type: "nvarchar(250)", maxLength: 250, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_IssueCategories", x => x.CategoryId);
                });

            migrationBuilder.CreateTable(
                name: "TicketStatuses",
                columns: table => new
                {
                    StatusId = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Name = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    Description = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_TicketStatuses", x => x.StatusId);
                });

            migrationBuilder.InsertData(
                table: "IssueCategories",
                columns: new[] { "CategoryId", "Description", "Name" },
                values: new object[,]
                {
                    { 1, "Technical issues", "Technical" },
                    { 2, "Billing and payments", "Billing" },
                    { 3, "Account-related issues", "Account" },
                    { 4, "General questions", "General Inquiry" },
                    { 5, "Other types of issues", "Other" }
                });

            migrationBuilder.InsertData(
                table: "TicketStatuses",
                columns: new[] { "StatusId", "Description", "Name" },
                values: new object[,]
                {
                    { 1, "Ticket just created", "New" },
                    { 2, "Assigned to an agent", "Assigned" },
                    { 3, "Work in progress", "InProgress" },
                    { 4, "Issue resolved", "Resolved" },
                    { 5, "Ticket closed", "Closed" }
                });

            migrationBuilder.CreateIndex(
                name: "IX_SupportTickets_CategoryId",
                table: "SupportTickets",
                column: "CategoryId");

            migrationBuilder.CreateIndex(
                name: "IX_SupportTickets_StatusId",
                table: "SupportTickets",
                column: "StatusId");

            migrationBuilder.AddForeignKey(
                name: "FK_SupportTickets_IssueCategories_CategoryId",
                table: "SupportTickets",
                column: "CategoryId",
                principalTable: "IssueCategories",
                principalColumn: "CategoryId",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_SupportTickets_TicketStatuses_StatusId",
                table: "SupportTickets",
                column: "StatusId",
                principalTable: "TicketStatuses",
                principalColumn: "StatusId",
                onDelete: ReferentialAction.Restrict);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_SupportTickets_IssueCategories_CategoryId",
                table: "SupportTickets");

            migrationBuilder.DropForeignKey(
                name: "FK_SupportTickets_TicketStatuses_StatusId",
                table: "SupportTickets");

            migrationBuilder.DropTable(
                name: "IssueCategories");

            migrationBuilder.DropTable(
                name: "TicketStatuses");

            migrationBuilder.DropIndex(
                name: "IX_SupportTickets_CategoryId",
                table: "SupportTickets");

            migrationBuilder.DropIndex(
                name: "IX_SupportTickets_StatusId",
                table: "SupportTickets");

            migrationBuilder.DropColumn(
                name: "CategoryId",
                table: "SupportTickets");

            migrationBuilder.DropColumn(
                name: "StatusId",
                table: "SupportTickets");

            migrationBuilder.AlterColumn<string>(
                name: "Description",
                table: "SupportTickets",
                type: "nvarchar(max)",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(2000)",
                oldMaxLength: 2000);

            migrationBuilder.AddColumn<string>(
                name: "IssueCategory",
                table: "SupportTickets",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "Status",
                table: "SupportTickets",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");
        }
    }
}
