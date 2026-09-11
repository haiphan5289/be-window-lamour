using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Lamour.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class DeactivateExtraWarehouses : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            // Theo yêu cầu: chỉ còn 2 Kho hoạt động (HH "Hàng hoá", TB "Trưng bày") cho việc chọn
            // Kho ở dòng sản phẩm — ngưng hoạt động "Kho chính" (id=1, seed từ
            // AddWarehouseReceipts) và "Kho chi nhánh Q.1" (id=3, không có trong seed migration nào
            // — dữ liệu chèn thủ công ngoài luồng, UpdateData vẫn an toàn no-op nếu DB khác không có
            // dòng này). Không xoá — chứng từ cũ đã tham chiếu 2 kho này vẫn giữ nguyên FK hợp lệ.
            migrationBuilder.UpdateData(
                table: "warehouses",
                keyColumn: "id",
                keyValue: 1,
                column: "is_active",
                value: false);

            migrationBuilder.UpdateData(
                table: "warehouses",
                keyColumn: "id",
                keyValue: 3,
                column: "is_active",
                value: false);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.UpdateData(
                table: "warehouses",
                keyColumn: "id",
                keyValue: 1,
                column: "is_active",
                value: true);

            migrationBuilder.UpdateData(
                table: "warehouses",
                keyColumn: "id",
                keyValue: 3,
                column: "is_active",
                value: true);
        }
    }
}
