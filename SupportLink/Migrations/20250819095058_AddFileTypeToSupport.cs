using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace SupportLink.Migrations
{
    /// <inheritdoc />
    public partial class AddFileTypeToSupport : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_SupportTickets_FileType_FileTypeId",
                table: "SupportTickets");

            migrationBuilder.DropPrimaryKey(
                name: "PK_FileType",
                table: "FileType");

            migrationBuilder.RenameTable(
                name: "FileType",
                newName: "FileTypes");

            migrationBuilder.AddPrimaryKey(
                name: "PK_FileTypes",
                table: "FileTypes",
                column: "FileTypeId");

            migrationBuilder.AddForeignKey(
                name: "FK_SupportTickets_FileTypes_FileTypeId",
                table: "SupportTickets",
                column: "FileTypeId",
                principalTable: "FileTypes",
                principalColumn: "FileTypeId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_SupportTickets_FileTypes_FileTypeId",
                table: "SupportTickets");

            migrationBuilder.DropPrimaryKey(
                name: "PK_FileTypes",
                table: "FileTypes");

            migrationBuilder.RenameTable(
                name: "FileTypes",
                newName: "FileType");

            migrationBuilder.AddPrimaryKey(
                name: "PK_FileType",
                table: "FileType",
                column: "FileTypeId");

            migrationBuilder.AddForeignKey(
                name: "FK_SupportTickets_FileType_FileTypeId",
                table: "SupportTickets",
                column: "FileTypeId",
                principalTable: "FileType",
                principalColumn: "FileTypeId");
        }
    }
}
