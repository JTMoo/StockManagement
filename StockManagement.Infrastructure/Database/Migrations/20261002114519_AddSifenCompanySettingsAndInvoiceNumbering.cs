using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace StockManagement.Infrastructure.Database.Migrations
{
    /// <inheritdoc />
    public partial class AddSifenCompanySettingsAndInvoiceNumbering : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            // Backfill existing plain sequential numbers into the "001"/"001" establishment/point-of-sale default, matching CompanySettings' own defaults
            migrationBuilder.DropIndex(name: "IX_Invoices_Number", table: "Invoices");
            migrationBuilder.Sql("""ALTER TABLE "Invoices" ALTER COLUMN "Number" TYPE text USING ('001-001-' || lpad("Number"::text, 7, '0'));""");
            migrationBuilder.CreateIndex(name: "IX_Invoices_Number", table: "Invoices", column: "Number", unique: true);

            migrationBuilder.AddColumn<string>(
                name: "EstablishmentCode",
                table: "AppSettings",
                type: "text",
                nullable: false,
                defaultValue: "001");

            migrationBuilder.AddColumn<string>(
                name: "PointOfSaleCode",
                table: "AppSettings",
                type: "text",
                nullable: false,
                defaultValue: "001");

            migrationBuilder.AddColumn<string>(
                name: "Ruc",
                table: "AppSettings",
                type: "text",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "TimbradoNumber",
                table: "AppSettings",
                type: "text",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<DateTime>(
                name: "TimbradoValidFrom",
                table: "AppSettings",
                type: "timestamp without time zone",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "TimbradoValidTo",
                table: "AppSettings",
                type: "timestamp without time zone",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "EstablishmentCode",
                table: "AppSettings");

            migrationBuilder.DropColumn(
                name: "PointOfSaleCode",
                table: "AppSettings");

            migrationBuilder.DropColumn(
                name: "Ruc",
                table: "AppSettings");

            migrationBuilder.DropColumn(
                name: "TimbradoNumber",
                table: "AppSettings");

            migrationBuilder.DropColumn(
                name: "TimbradoValidFrom",
                table: "AppSettings");

            migrationBuilder.DropColumn(
                name: "TimbradoValidTo",
                table: "AppSettings");

            migrationBuilder.DropIndex(name: "IX_Invoices_Number", table: "Invoices");
            migrationBuilder.Sql("""ALTER TABLE "Invoices" ALTER COLUMN "Number" TYPE integer USING (split_part("Number", '-', 3)::integer);""");
            migrationBuilder.CreateIndex(name: "IX_Invoices_Number", table: "Invoices", column: "Number", unique: true);
        }
    }
}
