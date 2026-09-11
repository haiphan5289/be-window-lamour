using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Lamour.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class SetDefaultWarehouseForExistingProducts : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            // Theo yêu cầu: set sẵn "Kho ngầm định" = Hàng hoá (HH) cho MỌI sản phẩm hiện có đang để
            // trống default_warehouse_id (xác nhận qua psql: 81/81 sản phẩm đều NULL) — chỉ là gợi ý
            // mặc định khi chọn hàng ở dòng chứng từ, KHÔNG đụng tới số lượng tồn kho thật
            // (product_warehouse_stocks giữ nguyên). Chỉ update dòng đang NULL — an toàn rerun, và
            // không ghi đè lựa chọn thủ công nếu user đã tự set Kho ngầm định khác trước đó.
            migrationBuilder.Sql(
                "UPDATE products SET default_warehouse_id = (SELECT id FROM warehouses WHERE code = 'HH') " +
                "WHERE default_warehouse_id IS NULL;");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql(
                "UPDATE products SET default_warehouse_id = NULL " +
                "WHERE default_warehouse_id = (SELECT id FROM warehouses WHERE code = 'HH');");
        }
    }
}
