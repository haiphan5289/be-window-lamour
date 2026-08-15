using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Lamour.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class CascadeDeleteProductWarehouseStock : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_product_warehouse_stocks_products_product_id",
                table: "product_warehouse_stocks");

            migrationBuilder.AddForeignKey(
                name: "FK_product_warehouse_stocks_products_product_id",
                table: "product_warehouse_stocks",
                column: "product_id",
                principalTable: "products",
                principalColumn: "id",
                onDelete: ReferentialAction.Cascade);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_product_warehouse_stocks_products_product_id",
                table: "product_warehouse_stocks");

            migrationBuilder.AddForeignKey(
                name: "FK_product_warehouse_stocks_products_product_id",
                table: "product_warehouse_stocks",
                column: "product_id",
                principalTable: "products",
                principalColumn: "id",
                onDelete: ReferentialAction.Restrict);
        }
    }
}
