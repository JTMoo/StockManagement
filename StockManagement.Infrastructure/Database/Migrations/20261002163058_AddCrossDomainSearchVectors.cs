using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace StockManagement.Infrastructure.Database.Migrations
{
    /// <inheritdoc />
    /// <remarks>
    /// ADR-0026: pg_trgm + generated tsvector/text columns, GIN-indexed, per searchable table.
    /// Unmapped in <see cref="AppDbContext"/> - read via raw SQL in search queries only, so no C# model change.
    /// Invoices can only generate from their own columns (Number); customer name/RUC is joined at query time.
    /// </remarks>
    public partial class AddCrossDomainSearchVectors : Migration
    {
        private static readonly (string Table, string Expression)[] Tables =
        [
            ("StockItems", "coalesce(\"Name\",'') || ' ' || coalesce(\"Code\",'') || ' ' || coalesce(\"Description\",'')"),
            ("Customers", "coalesce(\"Name\",'') || ' ' || coalesce(\"Lastname\",'') || ' ' || coalesce(\"IdentificationNumber\",'') || ' ' || coalesce(\"PhoneNumber\",'')"),
            ("Invoices", "coalesce(\"Number\",'')"),
            ("Suppliers", "coalesce(\"Name\",'') || ' ' || coalesce(\"ContactName\",'')"),
        ];

        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql("CREATE EXTENSION IF NOT EXISTS pg_trgm;");

            foreach (var (table, expression) in Tables)
            {
                migrationBuilder.Sql($"""
                    ALTER TABLE "{table}" ADD COLUMN "SearchText" text GENERATED ALWAYS AS ({expression}) STORED;
                    """);
                migrationBuilder.Sql($"""
                    ALTER TABLE "{table}" ADD COLUMN "SearchVector" tsvector GENERATED ALWAYS AS (to_tsvector('simple', {expression})) STORED;
                    """);
                migrationBuilder.Sql($"""
                    CREATE INDEX "IX_{table}_SearchVector" ON "{table}" USING GIN ("SearchVector");
                    """);
                migrationBuilder.Sql($"""
                    CREATE INDEX "IX_{table}_SearchText_Trgm" ON "{table}" USING GIN ("SearchText" gin_trgm_ops);
                    """);
            }
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            foreach (var (table, _) in Tables)
            {
                migrationBuilder.Sql($"""DROP INDEX IF EXISTS "IX_{table}_SearchText_Trgm";""");
                migrationBuilder.Sql($"""DROP INDEX IF EXISTS "IX_{table}_SearchVector";""");
                migrationBuilder.Sql($"""ALTER TABLE "{table}" DROP COLUMN IF EXISTS "SearchVector";""");
                migrationBuilder.Sql($"""ALTER TABLE "{table}" DROP COLUMN IF EXISTS "SearchText";""");
            }
        }
    }
}
