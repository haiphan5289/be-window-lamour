using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Lamour.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddPaymentDefaultAccounts6418And1111 : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            // 2026-09-29: Phiếu chi mặc định TK Nợ 6418 / TK Có 1111 (theo kế toán). SQL thô chỉ chèn khi
            // mã chưa có — không dùng HasData vì DB thật có thể đã có tài khoản người dùng tự tạo (trùng
            // id hoặc trùng mã), HasData với Id cố định sẽ làm migration lỗi trên máy khách.
            migrationBuilder.Sql(@"
                INSERT INTO account_settings (code, description)
                SELECT '6418', 'Chi phí bằng tiền khác'
                WHERE NOT EXISTS (SELECT 1 FROM account_settings WHERE lower(code) = '6418');

                INSERT INTO account_settings (code, description)
                SELECT '1111', 'Tiền Việt Nam'
                WHERE NOT EXISTS (SELECT 1 FROM account_settings WHERE lower(code) = '1111');
            ");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            // Chỉ xoá khi chưa có phiếu chi / sản phẩm nào dùng (FK Restrict) — tránh làm hỏng dữ liệu thật.
            migrationBuilder.Sql(@"
                DELETE FROM account_settings a
                WHERE a.code IN ('6418', '1111')
                  AND NOT EXISTS (SELECT 1 FROM payment_entries e
                                  WHERE e.""DebitAccountSettingId"" = a.id OR e.""CreditAccountSettingId"" = a.id)
                  AND NOT EXISTS (SELECT 1 FROM products p
                                  WHERE a.id IN (p.cost_account_id, p.discount_account_id, p.price_reduction_account_id,
                                                 p.return_account_id, p.revenue_account_id, p.stock_account_id));
            ");
        }
    }
}
