using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

#pragma warning disable CA1814 // Prefer jagged arrays over multidimensional

namespace Lamour.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddProductUnitsAndAccountSettings : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "account_settings",
                columns: table => new
                {
                    id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    code = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false),
                    description = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_account_settings", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "product_units",
                columns: table => new
                {
                    id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    name = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_product_units", x => x.id);
                });

            migrationBuilder.InsertData(
                table: "account_settings",
                columns: new[] { "id", "code", "description" },
                values: new object[,]
                {
                    { 1, "151", "Hàng mua đang đi đường" },
                    { 2, "152", "Nguyên liệu, vật liệu" },
                    { 3, "1531", "Công cụ, dụng cụ" },
                    { 4, "1532", "Bao bì luân chuyển" },
                    { 5, "1533", "Đồ dùng cho thuê" },
                    { 6, "1534", "Thiết bị, phụ tùng thay thế" },
                    { 7, "1551", "Thành phẩm nhập kho" },
                    { 8, "1557", "Thành phẩm bất động sản" },
                    { 9, "1561", "Giá mua hàng hóa" },
                    { 10, "1562", "Chi phí thu mua hàng hóa" },
                    { 11, "1567", "Hàng hóa bất động sản" },
                    { 12, "157", "Hàng gửi đi bán" },
                    { 13, "158", "Hàng hóa kho bảo thuế" },
                    { 14, "3339", "Phí, lệ phí và các khoản phải nộp khác" },
                    { 15, "5111", "Doanh thu bán hàng hóa" },
                    { 16, "5112", "Doanh thu bán các thành phẩm" },
                    { 17, "5113", "Doanh thu cung cấp dịch vụ" },
                    { 18, "5114", "Doanh thu trợ cấp, trợ giá" },
                    { 19, "5117", "Doanh thu kinh doanh bất động sản đầu tư" },
                    { 20, "5118", "Doanh thu khác" },
                    { 21, "711", "Thu nhập khác" },
                    { 22, "154", "Chi phí sản xuất, kinh doanh dở dang" },
                    { 23, "2411", "Mua sắm TSCĐ" },
                    { 24, "2412", "Xây dựng cơ bản" },
                    { 25, "2413", "Sửa chữa lớn TSCĐ" },
                    { 26, "242", "Chi phí trả trước" },
                    { 27, "6111", "Mua nguyên liệu, vật liệu" },
                    { 28, "6112", "Mua hàng hóa" },
                    { 29, "632", "Giá vốn hàng bán" },
                    { 30, "6232", "Chi phí vật liệu" },
                    { 31, "6412", "Chi phí vật liệu, bao bì" },
                    { 32, "6413", "Chi phí dụng cụ, đồ dùng" },
                    { 33, "6417", "Chi phí dịch vụ mua ngoài" },
                    { 34, "6422", "Chi phí vật liệu quản lý" },
                    { 35, "6423", "Chi phí đồ dùng văn phòng" },
                    { 36, "811", "Chi phí khác" }
                });

            migrationBuilder.InsertData(
                table: "product_units",
                columns: new[] { "id", "name" },
                values: new object[,]
                {
                    { 1, "Cái" },
                    { 2, "Hộp" },
                    { 3, "Chai" },
                    { 4, "Tuýp" },
                    { 5, "Cuốn" },
                    { 6, "Bộ" },
                    { 7, "Set" },
                    { 8, "Thùng" },
                    { 9, "Gói" },
                    { 10, "Lọ" }
                });

            migrationBuilder.CreateIndex(
                name: "IX_account_settings_code",
                table: "account_settings",
                column: "code",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_product_units_name",
                table: "product_units",
                column: "name",
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "account_settings");

            migrationBuilder.DropTable(
                name: "product_units");
        }
    }
}
