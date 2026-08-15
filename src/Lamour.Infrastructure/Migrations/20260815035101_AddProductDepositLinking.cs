using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Lamour.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddProductDepositLinking : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<bool>(
                name: "is_deposit_product",
                table: "products",
                type: "boolean",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<int>(
                name: "source_sales_order_id",
                table: "deposits",
                type: "integer",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_deposits_source_sales_order_id",
                table: "deposits",
                column: "source_sales_order_id");

            migrationBuilder.AddForeignKey(
                name: "FK_deposits_sales_orders_source_sales_order_id",
                table: "deposits",
                column: "source_sales_order_id",
                principalTable: "sales_orders",
                principalColumn: "id",
                onDelete: ReferentialAction.Restrict);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_deposits_sales_orders_source_sales_order_id",
                table: "deposits");

            migrationBuilder.DropIndex(
                name: "IX_deposits_source_sales_order_id",
                table: "deposits");

            migrationBuilder.DropColumn(
                name: "is_deposit_product",
                table: "products");

            migrationBuilder.DropColumn(
                name: "source_sales_order_id",
                table: "deposits");
        }
    }
}
