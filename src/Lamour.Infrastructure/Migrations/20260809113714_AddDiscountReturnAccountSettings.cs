using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

#pragma warning disable CA1814 // Prefer jagged arrays over multidimensional

namespace Lamour.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddDiscountReturnAccountSettings : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.InsertData(
                table: "account_settings",
                columns: new[] { "id", "code", "description" },
                values: new object[,]
                {
                    { 37, "5211", "Chiết khấu thương mại" },
                    { 38, "5212", "Hàng bán bị trả lại" },
                    { 39, "5213", "Giảm giá hàng bán" }
                });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DeleteData(
                table: "account_settings",
                keyColumn: "id",
                keyValue: 37);

            migrationBuilder.DeleteData(
                table: "account_settings",
                keyColumn: "id",
                keyValue: 38);

            migrationBuilder.DeleteData(
                table: "account_settings",
                keyColumn: "id",
                keyValue: 39);
        }
    }
}
