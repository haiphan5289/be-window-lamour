using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Lamour.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddIsDepositProductToSalesOrderLines : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<bool>(
                name: "is_deposit_product",
                table: "sales_order_lines",
                type: "boolean",
                nullable: false,
                defaultValue: false);

            // Backfill: dòng cũ được tạo trước khi có cột này — suy ra IsDepositProduct từ
            // Product.IsDepositProduct hiện tại (đúng ngay cả khi sản phẩm bị đổi tên sau này,
            // vì chỉ dựa vào ProductId, không dựa vào ProductName).
            migrationBuilder.Sql(@"
                UPDATE sales_order_lines sol
                SET is_deposit_product = true
                FROM products p
                WHERE sol.product_id = p.id AND p.is_deposit_product = true;
            ");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "is_deposit_product",
                table: "sales_order_lines");
        }
    }
}
