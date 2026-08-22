using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Lamour.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddSalesOrderIdToReceiptEntries : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "SalesOrderId",
                table: "receipt_entries",
                type: "integer",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_receipt_entries_SalesOrderId",
                table: "receipt_entries",
                column: "SalesOrderId");

            migrationBuilder.AddForeignKey(
                name: "FK_receipt_entries_sales_orders_SalesOrderId",
                table: "receipt_entries",
                column: "SalesOrderId",
                principalTable: "sales_orders",
                principalColumn: "id",
                onDelete: ReferentialAction.Restrict);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_receipt_entries_sales_orders_SalesOrderId",
                table: "receipt_entries");

            migrationBuilder.DropIndex(
                name: "IX_receipt_entries_SalesOrderId",
                table: "receipt_entries");

            migrationBuilder.DropColumn(
                name: "SalesOrderId",
                table: "receipt_entries");
        }
    }
}
