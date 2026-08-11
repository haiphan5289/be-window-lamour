using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace Lamour.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddProductWarehouseStock : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "warehouse_id",
                table: "sales_return_lines",
                type: "integer",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<int>(
                name: "warehouse_id",
                table: "sales_order_lines",
                type: "integer",
                nullable: false,
                defaultValue: 0);

            // Backfill warehouse_id cho các dòng Sales/SalesReturn đã tồn tại trước khi thêm FK NOT NULL:
            // ưu tiên DefaultWarehouseId của Product, fallback về kho "HH" (Hàng hoá).
            migrationBuilder.Sql(@"
                UPDATE sales_order_lines sol
                SET warehouse_id = COALESCE(p.default_warehouse_id, (SELECT id FROM warehouses WHERE code = 'HH' LIMIT 1))
                FROM products p
                WHERE p.id = sol.product_id;
            ");

            migrationBuilder.Sql(@"
                UPDATE sales_return_lines srl
                SET warehouse_id = COALESCE(p.default_warehouse_id, (SELECT id FROM warehouses WHERE code = 'HH' LIMIT 1))
                FROM products p
                WHERE p.id = srl.product_id;
            ");

            migrationBuilder.CreateTable(
                name: "product_warehouse_stocks",
                columns: table => new
                {
                    id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    product_id = table.Column<int>(type: "integer", nullable: false),
                    warehouse_id = table.Column<int>(type: "integer", nullable: false),
                    quantity = table.Column<int>(type: "integer", nullable: false, defaultValue: 0)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_product_warehouse_stocks", x => x.id);
                    table.ForeignKey(
                        name: "FK_product_warehouse_stocks_products_product_id",
                        column: x => x.product_id,
                        principalTable: "products",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_product_warehouse_stocks_warehouses_warehouse_id",
                        column: x => x.warehouse_id,
                        principalTable: "warehouses",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                });

            // Backfill 1 dòng ProductWarehouseStock cho mỗi Product hiện có, gán vào DefaultWarehouseId
            // (fallback "HH") với Quantity = StockQuantity hiện tại (snapshot, không replay lịch sử giao dịch).
            migrationBuilder.Sql(@"
                INSERT INTO product_warehouse_stocks (product_id, warehouse_id, quantity)
                SELECT p.id, COALESCE(p.default_warehouse_id, (SELECT id FROM warehouses WHERE code = 'HH' LIMIT 1)), p.stock_quantity
                FROM products p;
            ");

            migrationBuilder.CreateIndex(
                name: "IX_sales_return_lines_warehouse_id",
                table: "sales_return_lines",
                column: "warehouse_id");

            migrationBuilder.CreateIndex(
                name: "IX_sales_order_lines_warehouse_id",
                table: "sales_order_lines",
                column: "warehouse_id");

            migrationBuilder.CreateIndex(
                name: "IX_product_warehouse_stocks_product_id_warehouse_id",
                table: "product_warehouse_stocks",
                columns: new[] { "product_id", "warehouse_id" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_product_warehouse_stocks_warehouse_id",
                table: "product_warehouse_stocks",
                column: "warehouse_id");

            migrationBuilder.AddForeignKey(
                name: "FK_sales_order_lines_warehouses_warehouse_id",
                table: "sales_order_lines",
                column: "warehouse_id",
                principalTable: "warehouses",
                principalColumn: "id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_sales_return_lines_warehouses_warehouse_id",
                table: "sales_return_lines",
                column: "warehouse_id",
                principalTable: "warehouses",
                principalColumn: "id",
                onDelete: ReferentialAction.Restrict);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_sales_order_lines_warehouses_warehouse_id",
                table: "sales_order_lines");

            migrationBuilder.DropForeignKey(
                name: "FK_sales_return_lines_warehouses_warehouse_id",
                table: "sales_return_lines");

            migrationBuilder.DropTable(
                name: "product_warehouse_stocks");

            migrationBuilder.DropIndex(
                name: "IX_sales_return_lines_warehouse_id",
                table: "sales_return_lines");

            migrationBuilder.DropIndex(
                name: "IX_sales_order_lines_warehouse_id",
                table: "sales_order_lines");

            migrationBuilder.DropColumn(
                name: "warehouse_id",
                table: "sales_return_lines");

            migrationBuilder.DropColumn(
                name: "warehouse_id",
                table: "sales_order_lines");
        }
    }
}
