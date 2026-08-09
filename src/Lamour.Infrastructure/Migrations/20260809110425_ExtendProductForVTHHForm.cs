using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

#pragma warning disable CA1814 // Prefer jagged arrays over multidimensional

namespace Lamour.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class ExtendProductForVTHHForm : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "cost_account_id",
                table: "products",
                type: "integer",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "default_warehouse_id",
                table: "products",
                type: "integer",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "description",
                table: "products",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "discount_account_id",
                table: "products",
                type: "integer",
                nullable: true);

            migrationBuilder.AddColumn<bool>(
                name: "is_promotional_good",
                table: "products",
                type: "boolean",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<decimal>(
                name: "latest_purchase_price",
                table: "products",
                type: "numeric(18,2)",
                precision: 18,
                scale: 2,
                nullable: false,
                defaultValue: 0m);

            migrationBuilder.AddColumn<int>(
                name: "min_stock_quantity",
                table: "products",
                type: "integer",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<string>(
                name: "nature",
                table: "products",
                type: "character varying(20)",
                maxLength: 20,
                nullable: false,
                defaultValue: "VatTuHangHoa");

            migrationBuilder.AddColumn<string>(
                name: "origin",
                table: "products",
                type: "character varying(200)",
                maxLength: 200,
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "price_reduction_account_id",
                table: "products",
                type: "integer",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "product_unit_id",
                table: "products",
                type: "integer",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "purchase_description",
                table: "products",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "return_account_id",
                table: "products",
                type: "integer",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "revenue_account_id",
                table: "products",
                type: "integer",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "sale_description",
                table: "products",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "special_goods_type",
                table: "products",
                type: "character varying(100)",
                maxLength: 100,
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "stock_account_id",
                table: "products",
                type: "integer",
                nullable: true);

            migrationBuilder.AddColumn<decimal>(
                name: "trade_discount_rate",
                table: "products",
                type: "numeric(9,2)",
                precision: 9,
                scale: 2,
                nullable: false,
                defaultValue: 0m);

            migrationBuilder.AddColumn<string>(
                name: "warranty_period",
                table: "products",
                type: "character varying(100)",
                maxLength: 100,
                nullable: true);

            migrationBuilder.InsertData(
                table: "warehouses",
                columns: new[] { "id", "code", "is_active", "name" },
                values: new object[,]
                {
                    { 4, "HH", true, "Hàng hoá" },
                    { 5, "TB", true, "Trưng bày" }
                });

            migrationBuilder.CreateIndex(
                name: "IX_products_cost_account_id",
                table: "products",
                column: "cost_account_id");

            migrationBuilder.CreateIndex(
                name: "IX_products_default_warehouse_id",
                table: "products",
                column: "default_warehouse_id");

            migrationBuilder.CreateIndex(
                name: "IX_products_discount_account_id",
                table: "products",
                column: "discount_account_id");

            migrationBuilder.CreateIndex(
                name: "IX_products_price_reduction_account_id",
                table: "products",
                column: "price_reduction_account_id");

            migrationBuilder.CreateIndex(
                name: "IX_products_product_unit_id",
                table: "products",
                column: "product_unit_id");

            migrationBuilder.CreateIndex(
                name: "IX_products_return_account_id",
                table: "products",
                column: "return_account_id");

            migrationBuilder.CreateIndex(
                name: "IX_products_revenue_account_id",
                table: "products",
                column: "revenue_account_id");

            migrationBuilder.CreateIndex(
                name: "IX_products_stock_account_id",
                table: "products",
                column: "stock_account_id");

            migrationBuilder.AddForeignKey(
                name: "FK_products_account_settings_cost_account_id",
                table: "products",
                column: "cost_account_id",
                principalTable: "account_settings",
                principalColumn: "id",
                onDelete: ReferentialAction.SetNull);

            migrationBuilder.AddForeignKey(
                name: "FK_products_account_settings_discount_account_id",
                table: "products",
                column: "discount_account_id",
                principalTable: "account_settings",
                principalColumn: "id",
                onDelete: ReferentialAction.SetNull);

            migrationBuilder.AddForeignKey(
                name: "FK_products_account_settings_price_reduction_account_id",
                table: "products",
                column: "price_reduction_account_id",
                principalTable: "account_settings",
                principalColumn: "id",
                onDelete: ReferentialAction.SetNull);

            migrationBuilder.AddForeignKey(
                name: "FK_products_account_settings_return_account_id",
                table: "products",
                column: "return_account_id",
                principalTable: "account_settings",
                principalColumn: "id",
                onDelete: ReferentialAction.SetNull);

            migrationBuilder.AddForeignKey(
                name: "FK_products_account_settings_revenue_account_id",
                table: "products",
                column: "revenue_account_id",
                principalTable: "account_settings",
                principalColumn: "id",
                onDelete: ReferentialAction.SetNull);

            migrationBuilder.AddForeignKey(
                name: "FK_products_account_settings_stock_account_id",
                table: "products",
                column: "stock_account_id",
                principalTable: "account_settings",
                principalColumn: "id",
                onDelete: ReferentialAction.SetNull);

            migrationBuilder.AddForeignKey(
                name: "FK_products_product_units_product_unit_id",
                table: "products",
                column: "product_unit_id",
                principalTable: "product_units",
                principalColumn: "id",
                onDelete: ReferentialAction.SetNull);

            migrationBuilder.AddForeignKey(
                name: "FK_products_warehouses_default_warehouse_id",
                table: "products",
                column: "default_warehouse_id",
                principalTable: "warehouses",
                principalColumn: "id",
                onDelete: ReferentialAction.SetNull);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_products_account_settings_cost_account_id",
                table: "products");

            migrationBuilder.DropForeignKey(
                name: "FK_products_account_settings_discount_account_id",
                table: "products");

            migrationBuilder.DropForeignKey(
                name: "FK_products_account_settings_price_reduction_account_id",
                table: "products");

            migrationBuilder.DropForeignKey(
                name: "FK_products_account_settings_return_account_id",
                table: "products");

            migrationBuilder.DropForeignKey(
                name: "FK_products_account_settings_revenue_account_id",
                table: "products");

            migrationBuilder.DropForeignKey(
                name: "FK_products_account_settings_stock_account_id",
                table: "products");

            migrationBuilder.DropForeignKey(
                name: "FK_products_product_units_product_unit_id",
                table: "products");

            migrationBuilder.DropForeignKey(
                name: "FK_products_warehouses_default_warehouse_id",
                table: "products");

            migrationBuilder.DropIndex(
                name: "IX_products_cost_account_id",
                table: "products");

            migrationBuilder.DropIndex(
                name: "IX_products_default_warehouse_id",
                table: "products");

            migrationBuilder.DropIndex(
                name: "IX_products_discount_account_id",
                table: "products");

            migrationBuilder.DropIndex(
                name: "IX_products_price_reduction_account_id",
                table: "products");

            migrationBuilder.DropIndex(
                name: "IX_products_product_unit_id",
                table: "products");

            migrationBuilder.DropIndex(
                name: "IX_products_return_account_id",
                table: "products");

            migrationBuilder.DropIndex(
                name: "IX_products_revenue_account_id",
                table: "products");

            migrationBuilder.DropIndex(
                name: "IX_products_stock_account_id",
                table: "products");

            migrationBuilder.DeleteData(
                table: "warehouses",
                keyColumn: "id",
                keyValue: 4);

            migrationBuilder.DeleteData(
                table: "warehouses",
                keyColumn: "id",
                keyValue: 5);

            migrationBuilder.DropColumn(
                name: "cost_account_id",
                table: "products");

            migrationBuilder.DropColumn(
                name: "default_warehouse_id",
                table: "products");

            migrationBuilder.DropColumn(
                name: "description",
                table: "products");

            migrationBuilder.DropColumn(
                name: "discount_account_id",
                table: "products");

            migrationBuilder.DropColumn(
                name: "is_promotional_good",
                table: "products");

            migrationBuilder.DropColumn(
                name: "latest_purchase_price",
                table: "products");

            migrationBuilder.DropColumn(
                name: "min_stock_quantity",
                table: "products");

            migrationBuilder.DropColumn(
                name: "nature",
                table: "products");

            migrationBuilder.DropColumn(
                name: "origin",
                table: "products");

            migrationBuilder.DropColumn(
                name: "price_reduction_account_id",
                table: "products");

            migrationBuilder.DropColumn(
                name: "product_unit_id",
                table: "products");

            migrationBuilder.DropColumn(
                name: "purchase_description",
                table: "products");

            migrationBuilder.DropColumn(
                name: "return_account_id",
                table: "products");

            migrationBuilder.DropColumn(
                name: "revenue_account_id",
                table: "products");

            migrationBuilder.DropColumn(
                name: "sale_description",
                table: "products");

            migrationBuilder.DropColumn(
                name: "special_goods_type",
                table: "products");

            migrationBuilder.DropColumn(
                name: "stock_account_id",
                table: "products");

            migrationBuilder.DropColumn(
                name: "trade_discount_rate",
                table: "products");

            migrationBuilder.DropColumn(
                name: "warranty_period",
                table: "products");
        }
    }
}
