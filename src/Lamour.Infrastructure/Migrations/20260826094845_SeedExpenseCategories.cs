using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Lamour.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class SeedExpenseCategories : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            // Khớp ảnh mẫu MISA "Khoản mục CP" trên popup Phiếu chi — 8 phòng ban, thay cho dòng
            // seed rác "111"/"sale" đã có sẵn trong DB (giữ nguyên, không xoá — dữ liệu người dùng
            // đã tạo, không phải seed của hệ thống).
            migrationBuilder.InsertData(
                table: "expense_categories",
                columns: new[] { "code", "name" },
                values: new object[,]
                {
                    { "01", "PHÒNG SALES" },
                    { "02", "PHÒNG MARKETING" },
                    { "03", "PHÒNG KHO VẬN" },
                    { "04", "PHÒNG TÀI CHÍNH - KẾ TOÁN" },
                    { "05", "PHÒNG NHÂN SỰ" },
                    { "06", "PHÒNG ĐÀO TẠO" },
                    { "07", "PHÒNG SPA" },
                    { "08", "KHÁC" },
                });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DeleteData(
                table: "expense_categories",
                keyColumn: "code",
                keyValues: new object[] { "01", "02", "03", "04", "05", "06", "07", "08" });
        }
    }
}
