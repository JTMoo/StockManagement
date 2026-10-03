using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace StockManagement.Infrastructure.Database.Migrations
{
    /// <inheritdoc />
    public partial class AddRemissionNotes : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "RemissionNotes",
                columns: table => new
                {
                    Id = table.Column<string>(type: "text", nullable: false),
                    Number = table.Column<string>(type: "text", nullable: false),
                    Date = table.Column<DateTime>(type: "timestamp without time zone", nullable: false),
                    Reason = table.Column<int>(type: "integer", nullable: false),
                    DestinationAddress = table.Column<string>(type: "text", nullable: false),
                    CustomerId = table.Column<string>(type: "text", nullable: false),
                    Cdc = table.Column<string>(type: "text", nullable: false),
                    TransmissionStatus = table.Column<int>(type: "integer", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_RemissionNotes", x => x.Id);
                    table.ForeignKey(
                        name: "FK_RemissionNotes_Customers_CustomerId",
                        column: x => x.CustomerId,
                        principalTable: "Customers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "PendingRemisionTransmissions",
                columns: table => new
                {
                    Id = table.Column<string>(type: "text", nullable: false),
                    RemissionNoteId = table.Column<string>(type: "text", nullable: false),
                    Attempts = table.Column<int>(type: "integer", nullable: false),
                    NextAttemptAt = table.Column<DateTime>(type: "timestamp without time zone", nullable: false),
                    LastError = table.Column<string>(type: "text", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_PendingRemisionTransmissions", x => x.Id);
                    table.ForeignKey(
                        name: "FK_PendingRemisionTransmissions_RemissionNotes_RemissionNoteId",
                        column: x => x.RemissionNoteId,
                        principalTable: "RemissionNotes",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "RemissionNoteItems",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    StockItemId = table.Column<string>(type: "text", nullable: false),
                    Amount = table.Column<int>(type: "integer", nullable: false),
                    RemissionNoteId = table.Column<string>(type: "text", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_RemissionNoteItems", x => x.Id);
                    table.ForeignKey(
                        name: "FK_RemissionNoteItems_RemissionNotes_RemissionNoteId",
                        column: x => x.RemissionNoteId,
                        principalTable: "RemissionNotes",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_RemissionNoteItems_StockItems_StockItemId",
                        column: x => x.StockItemId,
                        principalTable: "StockItems",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_PendingRemisionTransmissions_RemissionNoteId",
                table: "PendingRemisionTransmissions",
                column: "RemissionNoteId");

            migrationBuilder.CreateIndex(
                name: "IX_RemissionNoteItems_RemissionNoteId",
                table: "RemissionNoteItems",
                column: "RemissionNoteId");

            migrationBuilder.CreateIndex(
                name: "IX_RemissionNoteItems_StockItemId",
                table: "RemissionNoteItems",
                column: "StockItemId");

            migrationBuilder.CreateIndex(
                name: "IX_RemissionNotes_CustomerId",
                table: "RemissionNotes",
                column: "CustomerId");

            migrationBuilder.CreateIndex(
                name: "IX_RemissionNotes_Number",
                table: "RemissionNotes",
                column: "Number",
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "PendingRemisionTransmissions");

            migrationBuilder.DropTable(
                name: "RemissionNoteItems");

            migrationBuilder.DropTable(
                name: "RemissionNotes");
        }
    }
}
