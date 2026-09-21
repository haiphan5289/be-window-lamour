using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Lamour.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class MarkTruCocProductAsDeposit : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            // "Trừ Cọc" (mã 36) giờ là dòng hàng thường chỉ có Thành tiền (âm), không kho/tồn kho —
            // dùng chung đường xử lý của sản phẩm cọc (IsDepositProduct).
            migrationBuilder.Sql("UPDATE products SET is_deposit_product = true WHERE code = '36';");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql("UPDATE products SET is_deposit_product = false WHERE code = '36';");
        }
    }
}
