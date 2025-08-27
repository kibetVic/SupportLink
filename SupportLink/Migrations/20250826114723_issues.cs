using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace SupportLink.Migrations
{
    /// <inheritdoc />
    public partial class issues : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.UpdateData(
                table: "IssueCategories",
                keyColumn: "CategoryId",
                keyValue: 5,
                columns: new[] { "Description", "Name" },
                values: new object[] { "World Bank questions", "World Bank issues" });

            migrationBuilder.InsertData(
                table: "IssueCategories",
                columns: new[] { "CategoryId", "Description", "Name" },
                values: new object[] { 6, "Other types of issues", "Other" });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DeleteData(
                table: "IssueCategories",
                keyColumn: "CategoryId",
                keyValue: 6);

            migrationBuilder.UpdateData(
                table: "IssueCategories",
                keyColumn: "CategoryId",
                keyValue: 5,
                columns: new[] { "Description", "Name" },
                values: new object[] { "Other types of issues", "Other" });
        }
    }
}
