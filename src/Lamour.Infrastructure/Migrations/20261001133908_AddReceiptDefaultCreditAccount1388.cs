using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Lamour.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddReceiptDefaultCreditAccount1388 : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            // 2026-10-01: Phiếu thu mặc định TK Có 1388 (theo kế toán, khớp MISA). Chỉ chèn khi mã chưa có —
            // cùng cách AddPaymentDefaultAccounts6418And1111.
            migrationBuilder.Sql(@"
                INSERT INTO account_settings (code, description)
                SELECT '1388', 'Phải thu khác'
                WHERE NOT EXISTS (SELECT 1 FROM account_settings WHERE lower(code) = '1388');
            ");

        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            // Chỉ xoá khi chưa có phiếu thu/chi hay sản phẩm nào dùng (FK Restrict).
            migrationBuilder.Sql(@"
                DELETE FROM account_settings a
                WHERE a.code = '1388'
                  AND NOT EXISTS (SELECT 1 FROM receipt_entries e
                                  WHERE e.""DebitAccountSettingId"" = a.id OR e.""CreditAccountSettingId"" = a.id)
                  AND NOT EXISTS (SELECT 1 FROM payment_entries e
                                  WHERE e.""DebitAccountSettingId"" = a.id OR e.""CreditAccountSettingId"" = a.id)
                  AND NOT EXISTS (SELECT 1 FROM products p
                                  WHERE a.id IN (p.cost_account_id, p.discount_account_id, p.price_reduction_account_id,
                                                 p.return_account_id, p.revenue_account_id, p.stock_account_id));
            ");

        }
    }
}
