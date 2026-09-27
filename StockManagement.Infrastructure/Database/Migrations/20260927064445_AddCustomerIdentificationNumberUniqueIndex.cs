using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace StockManagement.Infrastructure.Database.Migrations
{
    /// <inheritdoc />
    public partial class AddCustomerIdentificationNumberUniqueIndex : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            // Legacy data can already have the same IdentificationNumber on more than one customer; clear it on all but the
            // lowest CustomerId per value so the unique index below can be created, rather than failing the migration.
            migrationBuilder.Sql(
                """
                DO $$
                DECLARE
                    duplicate RECORD;
                BEGIN
                    FOR duplicate IN
                        SELECT "Id", "CustomerId", "IdentificationNumber"
                        FROM "Customers" c
                        WHERE "IdentificationNumber" <> ''
                          AND EXISTS (
                              SELECT 1 FROM "Customers" c2
                              WHERE c2."IdentificationNumber" = c."IdentificationNumber"
                                AND c2."CustomerId" < c."CustomerId"
                          )
                    LOOP
                        RAISE NOTICE 'Clearing duplicate IdentificationNumber % on customer %', duplicate."IdentificationNumber", duplicate."CustomerId";
                        UPDATE "Customers" SET "IdentificationNumber" = '' WHERE "Id" = duplicate."Id";
                    END LOOP;
                END $$;
                """);

            migrationBuilder.CreateIndex(
                name: "IX_Customers_IdentificationNumber",
                table: "Customers",
                column: "IdentificationNumber",
                unique: true,
                filter: "\"IdentificationNumber\" <> ''");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_Customers_IdentificationNumber",
                table: "Customers");
        }
    }
}
