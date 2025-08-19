using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

#pragma warning disable CA1814 // Prefer jagged arrays over multidimensional

namespace SupportLink.Migrations
{
    /// <inheritdoc />
    public partial class AddColumnFiles : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "FileType",
                table: "SupportTickets",
                newName: "FileTypeId");

            migrationBuilder.CreateTable(
                name: "FileType",
                columns: table => new
                {
                    FileTypeId = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Name = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    Description = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_FileType", x => x.FileTypeId);
                });

            migrationBuilder.InsertData(
                table: "FileType",
                columns: new[] { "FileTypeId", "Description", "Name" },
                values: new object[,]
                {
                    { 1, null, "Image" },
                    { 2, null, "Document" },
                    { 3, null, "PDF" },
                    { 4, null, "Video" },
                    { 5, null, "Others" }
                });

            migrationBuilder.CreateIndex(
                name: "IX_SupportTickets_FileTypeId",
                table: "SupportTickets",
                column: "FileTypeId");

            migrationBuilder.AddForeignKey(
                name: "FK_SupportTickets_FileType_FileTypeId",
                table: "SupportTickets",
                column: "FileTypeId",
                principalTable: "FileType",
                principalColumn: "FileTypeId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_SupportTickets_FileType_FileTypeId",
                table: "SupportTickets");

            migrationBuilder.DropTable(
                name: "FileType");

            migrationBuilder.DropIndex(
                name: "IX_SupportTickets_FileTypeId",
                table: "SupportTickets");

            migrationBuilder.RenameColumn(
                name: "FileTypeId",
                table: "SupportTickets",
                newName: "FileType");
        }
    }
}
