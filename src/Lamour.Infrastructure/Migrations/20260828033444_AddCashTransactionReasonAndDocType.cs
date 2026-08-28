using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Lamour.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddCashTransactionReasonAndDocType : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "document_type",
                table: "cash_transactions",
                type: "character varying(100)",
                maxLength: 100,
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "payment_reason",
                table: "cash_transactions",
                type: "character varying(30)",
                maxLength: 30,
                nullable: true);

            migrationBuilder.UpdateData(
                table: "cash_transactions",
                keyColumn: "id",
                keyValue: 1,
                columns: new[] { "document_type", "payment_reason" },
                values: new object[] { "Phiếu thu tiền mặt khách hàng", "ThuTienHang" });

            migrationBuilder.UpdateData(
                table: "cash_transactions",
                keyColumn: "id",
                keyValue: 2,
                columns: new[] { "document_type", "payment_reason" },
                values: new object[] { "Phiếu thu tiền mặt khách hàng", "ThuTienHang" });

            migrationBuilder.UpdateData(
                table: "cash_transactions",
                keyColumn: "id",
                keyValue: 3,
                columns: new[] { "document_type", "payment_reason" },
                values: new object[] { "Phiếu thu tiền mặt khách hàng", "ThuTienHang" });

            migrationBuilder.UpdateData(
                table: "cash_transactions",
                keyColumn: "id",
                keyValue: 4,
                columns: new[] { "document_type", "payment_reason" },
                values: new object[] { "Phiếu thu tiền mặt khách hàng", "ThuTienHang" });

            migrationBuilder.UpdateData(
                table: "cash_transactions",
                keyColumn: "id",
                keyValue: 5,
                columns: new[] { "document_type", "payment_reason" },
                values: new object[] { "Phiếu thu tiền mặt khách hàng", "ThuTienHang" });

            migrationBuilder.UpdateData(
                table: "cash_transactions",
                keyColumn: "id",
                keyValue: 6,
                columns: new[] { "document_type", "payment_reason" },
                values: new object[] { "Phiếu chi", "ChiKhac" });

            migrationBuilder.UpdateData(
                table: "cash_transactions",
                keyColumn: "id",
                keyValue: 7,
                columns: new[] { "document_type", "payment_reason" },
                values: new object[] { "Phiếu chi", "ChiKhac" });

            migrationBuilder.UpdateData(
                table: "cash_transactions",
                keyColumn: "id",
                keyValue: 8,
                columns: new[] { "document_type", "payment_reason" },
                values: new object[] { "Phiếu chi", "ChiKhac" });

            migrationBuilder.UpdateData(
                table: "cash_transactions",
                keyColumn: "id",
                keyValue: 9,
                columns: new[] { "document_type", "payment_reason" },
                values: new object[] { "Phiếu chi", "ChiKhac" });

            migrationBuilder.UpdateData(
                table: "cash_transactions",
                keyColumn: "id",
                keyValue: 10,
                columns: new[] { "document_type", "payment_reason" },
                values: new object[] { "Phiếu chi", "ChiKhac" });

            migrationBuilder.UpdateData(
                table: "cash_transactions",
                keyColumn: "id",
                keyValue: 11,
                columns: new[] { "document_type", "payment_reason" },
                values: new object[] { "Phiếu chi", "ChiKhac" });

            migrationBuilder.UpdateData(
                table: "cash_transactions",
                keyColumn: "id",
                keyValue: 12,
                columns: new[] { "document_type", "payment_reason" },
                values: new object[] { "Phiếu chi", "ChiKhac" });

            migrationBuilder.UpdateData(
                table: "cash_transactions",
                keyColumn: "id",
                keyValue: 13,
                columns: new[] { "document_type", "payment_reason" },
                values: new object[] { "Phiếu chi", "ChiKhac" });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "document_type",
                table: "cash_transactions");

            migrationBuilder.DropColumn(
                name: "payment_reason",
                table: "cash_transactions");
        }
    }
}
