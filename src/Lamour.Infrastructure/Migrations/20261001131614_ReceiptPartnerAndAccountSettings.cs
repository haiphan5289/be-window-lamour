using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Lamour.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class ReceiptPartnerAndAccountSettings : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            // 2026-10-01: Phiếu thu theo luồng MISA (giống Phiếu chi) — TK Nợ/Có chuyển từ 4 giá trị cứng
            // (Cash111/Bank112/Receivable131/Payroll334) sang FK danh mục tài khoản, thêm Đối tượng đa loại
            // (Khách hàng / Nhân viên) và "Lý do nộp" chi tiết. Thứ tự bắt buộc: thêm cột mới → chuyển dữ
            // liệu cũ → MỚI xoá cột cũ và gắn FK (scaffold mặc định xoá cột cũ trước = mất dữ liệu).
            migrationBuilder.AddColumn<int>(
                name: "PartnerId",
                table: "receipts",
                type: "integer",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "PartnerName",
                table: "receipts",
                type: "character varying(200)",
                maxLength: 200,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "PartnerType",
                table: "receipts",
                type: "character varying(20)",
                maxLength: 20,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "ReasonDetail",
                table: "receipts",
                type: "character varying(500)",
                maxLength: 500,
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "CreditAccountSettingId",
                table: "receipt_entries",
                type: "integer",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<int>(
                name: "DebitAccountSettingId",
                table: "receipt_entries",
                type: "integer",
                nullable: false,
                defaultValue: 0);

            // Chèn các TK mà dữ liệu cũ cần nếu danh mục chưa có (không dùng HasData — xem
            // AddPaymentDefaultAccounts6418And1111).
            migrationBuilder.Sql(@"
                INSERT INTO account_settings (code, description)
                SELECT v.code, v.description
                FROM (VALUES ('1111', 'Tiền Việt Nam'),
                             ('1121', 'Tiền gửi ngân hàng - Tiền Việt Nam'),
                             ('131',  'Phải thu của khách hàng'),
                             ('334',  'Phải trả người lao động')) AS v(code, description)
                WHERE NOT EXISTS (SELECT 1 FROM account_settings a WHERE lower(a.code) = v.code);

                UPDATE receipt_entries e
                SET ""DebitAccountSettingId"" = (
                        SELECT a.id FROM account_settings a
                        WHERE a.code = CASE e.""DebitAccount""
                                           WHEN 'Bank112'       THEN '1121'
                                           WHEN 'Receivable131' THEN '131'
                                           WHEN 'Payroll334'    THEN '334'
                                           ELSE '1111' END),
                    ""CreditAccountSettingId"" = (
                        SELECT a.id FROM account_settings a
                        WHERE a.code = CASE e.""CreditAccount""
                                           WHEN 'Cash111'    THEN '1111'
                                           WHEN 'Bank112'    THEN '1121'
                                           WHEN 'Payroll334' THEN '334'
                                           ELSE '131' END);

                -- Phiếu thu 1 khách hàng cũ → Đối tượng = Khách hàng. Phiếu có CustomerId NULL là phiếu
                -- thu hàng loạt: giữ PartnerType NULL (Receipt.IsBulk).
                UPDATE receipts r
                SET ""PartnerType"" = 'Customer',
                    ""PartnerId""   = r.""CustomerId"",
                    ""PartnerName"" = c.name
                FROM customers c
                WHERE c.id = r.""CustomerId"";
            ");

            migrationBuilder.DropColumn(
                name: "CreditAccount",
                table: "receipt_entries");

            migrationBuilder.DropColumn(
                name: "DebitAccount",
                table: "receipt_entries");

            migrationBuilder.CreateIndex(
                name: "IX_receipts_PartnerType_PartnerId",
                table: "receipts",
                columns: new[] { "PartnerType", "PartnerId" });

            migrationBuilder.CreateIndex(
                name: "IX_receipt_entries_CreditAccountSettingId",
                table: "receipt_entries",
                column: "CreditAccountSettingId");

            migrationBuilder.CreateIndex(
                name: "IX_receipt_entries_DebitAccountSettingId",
                table: "receipt_entries",
                column: "DebitAccountSettingId");

            migrationBuilder.AddForeignKey(
                name: "FK_receipt_entries_account_settings_CreditAccountSettingId",
                table: "receipt_entries",
                column: "CreditAccountSettingId",
                principalTable: "account_settings",
                principalColumn: "id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_receipt_entries_account_settings_DebitAccountSettingId",
                table: "receipt_entries",
                column: "DebitAccountSettingId",
                principalTable: "account_settings",
                principalColumn: "id",
                onDelete: ReferentialAction.Restrict);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_receipt_entries_account_settings_CreditAccountSettingId",
                table: "receipt_entries");

            migrationBuilder.DropForeignKey(
                name: "FK_receipt_entries_account_settings_DebitAccountSettingId",
                table: "receipt_entries");

            migrationBuilder.DropIndex(
                name: "IX_receipts_PartnerType_PartnerId",
                table: "receipts");

            migrationBuilder.DropIndex(
                name: "IX_receipt_entries_CreditAccountSettingId",
                table: "receipt_entries");

            migrationBuilder.DropIndex(
                name: "IX_receipt_entries_DebitAccountSettingId",
                table: "receipt_entries");

            migrationBuilder.AddColumn<string>(
                name: "CreditAccount",
                table: "receipt_entries",
                type: "character varying(20)",
                maxLength: 20,
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "DebitAccount",
                table: "receipt_entries",
                type: "character varying(20)",
                maxLength: 20,
                nullable: false,
                defaultValue: "");

            // Mã TK không thuộc 4 giá trị cũ (vd 1388) không biểu diễn được → rơi về 111/131.
            migrationBuilder.Sql(@"
                UPDATE receipt_entries e
                SET ""DebitAccount"" = COALESCE((
                        SELECT CASE WHEN a.code LIKE '112%' THEN 'Bank112'
                                    WHEN a.code LIKE '131%' THEN 'Receivable131'
                                    WHEN a.code LIKE '334%' THEN 'Payroll334'
                                    ELSE 'Cash111' END
                        FROM account_settings a WHERE a.id = e.""DebitAccountSettingId""), 'Cash111'),
                    ""CreditAccount"" = COALESCE((
                        SELECT CASE WHEN a.code LIKE '111%' THEN 'Cash111'
                                    WHEN a.code LIKE '112%' THEN 'Bank112'
                                    WHEN a.code LIKE '334%' THEN 'Payroll334'
                                    ELSE 'Receivable131' END
                        FROM account_settings a WHERE a.id = e.""CreditAccountSettingId""), 'Receivable131');
            ");

            migrationBuilder.DropColumn(
                name: "PartnerId",
                table: "receipts");

            migrationBuilder.DropColumn(
                name: "PartnerName",
                table: "receipts");

            migrationBuilder.DropColumn(
                name: "PartnerType",
                table: "receipts");

            migrationBuilder.DropColumn(
                name: "ReasonDetail",
                table: "receipts");

            migrationBuilder.DropColumn(
                name: "CreditAccountSettingId",
                table: "receipt_entries");

            migrationBuilder.DropColumn(
                name: "DebitAccountSettingId",
                table: "receipt_entries");
        }
    }
}
