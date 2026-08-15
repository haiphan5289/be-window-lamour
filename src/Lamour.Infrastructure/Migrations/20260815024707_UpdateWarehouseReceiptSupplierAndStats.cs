using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Lamour.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class UpdateWarehouseReceiptSupplierAndStats : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "supplier_id",
                table: "warehouse_receipts",
                type: "integer",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "cost_item",
                table: "warehouse_receipt_lines",
                type: "character varying(100)",
                maxLength: 100,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "cost_object",
                table: "warehouse_receipt_lines",
                type: "character varying(100)",
                maxLength: 100,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "loan_contract_number",
                table: "warehouse_receipt_lines",
                type: "character varying(100)",
                maxLength: 100,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "project",
                table: "warehouse_receipt_lines",
                type: "character varying(100)",
                maxLength: 100,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "purchase_order_number",
                table: "warehouse_receipt_lines",
                type: "character varying(100)",
                maxLength: 100,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "sales_contract_number",
                table: "warehouse_receipt_lines",
                type: "character varying(100)",
                maxLength: 100,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "statistics_code",
                table: "warehouse_receipt_lines",
                type: "character varying(100)",
                maxLength: 100,
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_warehouse_receipts_supplier_id",
                table: "warehouse_receipts",
                column: "supplier_id");

            migrationBuilder.AddForeignKey(
                name: "FK_warehouse_receipts_suppliers_supplier_id",
                table: "warehouse_receipts",
                column: "supplier_id",
                principalTable: "suppliers",
                principalColumn: "id",
                onDelete: ReferentialAction.Restrict);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_warehouse_receipts_suppliers_supplier_id",
                table: "warehouse_receipts");

            migrationBuilder.DropIndex(
                name: "IX_warehouse_receipts_supplier_id",
                table: "warehouse_receipts");

            migrationBuilder.DropColumn(
                name: "supplier_id",
                table: "warehouse_receipts");

            migrationBuilder.DropColumn(
                name: "cost_item",
                table: "warehouse_receipt_lines");

            migrationBuilder.DropColumn(
                name: "cost_object",
                table: "warehouse_receipt_lines");

            migrationBuilder.DropColumn(
                name: "loan_contract_number",
                table: "warehouse_receipt_lines");

            migrationBuilder.DropColumn(
                name: "project",
                table: "warehouse_receipt_lines");

            migrationBuilder.DropColumn(
                name: "purchase_order_number",
                table: "warehouse_receipt_lines");

            migrationBuilder.DropColumn(
                name: "sales_contract_number",
                table: "warehouse_receipt_lines");

            migrationBuilder.DropColumn(
                name: "statistics_code",
                table: "warehouse_receipt_lines");
        }
    }
}
