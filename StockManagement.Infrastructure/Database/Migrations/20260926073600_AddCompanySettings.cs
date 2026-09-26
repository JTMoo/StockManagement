using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace StockManagement.Infrastructure.Database.Migrations
{
    /// <inheritdoc />
    public partial class AddCompanySettings : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "CompanyName",
                table: "AppSettings",
                type: "text",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "Currency",
                table: "AppSettings",
                type: "text",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<int>(
                name: "FirstCustomerId",
                table: "AppSettings",
                type: "integer",
                nullable: false,
                defaultValue: 1001);

            migrationBuilder.AddColumn<int>(
                name: "FirstInvoiceNumber",
                table: "AppSettings",
                type: "integer",
                nullable: false,
                defaultValue: 1);

            migrationBuilder.AddColumn<int>(
                name: "PaymentTermInDays",
                table: "AppSettings",
                type: "integer",
                nullable: false,
                defaultValue: 30);

            migrationBuilder.AddColumn<string>(
                name: "TaxId",
                table: "AppSettings",
                type: "text",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<decimal>(
                name: "VatRatePercent",
                table: "AppSettings",
                type: "numeric(5,2)",
                nullable: false,
                defaultValue: 10m);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "CompanyName",
                table: "AppSettings");

            migrationBuilder.DropColumn(
                name: "Currency",
                table: "AppSettings");

            migrationBuilder.DropColumn(
                name: "FirstCustomerId",
                table: "AppSettings");

            migrationBuilder.DropColumn(
                name: "FirstInvoiceNumber",
                table: "AppSettings");

            migrationBuilder.DropColumn(
                name: "PaymentTermInDays",
                table: "AppSettings");

            migrationBuilder.DropColumn(
                name: "TaxId",
                table: "AppSettings");

            migrationBuilder.DropColumn(
                name: "VatRatePercent",
                table: "AppSettings");
        }
    }
}
