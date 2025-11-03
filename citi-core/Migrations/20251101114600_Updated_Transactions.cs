using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace citi_core.Migrations
{
    /// <inheritdoc />
    public partial class Updated_Transactions : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "BillerName",
                table: "Transactions");

            migrationBuilder.DropColumn(
                name: "BillerReference",
                table: "Transactions");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "BillerName",
                table: "Transactions",
                type: "nvarchar(100)",
                maxLength: 100,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "BillerReference",
                table: "Transactions",
                type: "nvarchar(50)",
                maxLength: 50,
                nullable: true);
        }
    }
}
