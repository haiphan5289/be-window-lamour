using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Lamour.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddSalesReturnTaxAndGrandTotal : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<decimal>(
                name: "grand_total",
                table: "sales_returns",
                type: "numeric(18,2)",
                precision: 18,
                scale: 2,
                nullable: false,
                defaultValue: 0m);

            migrationBuilder.AddColumn<decimal>(
                name: "total_tax_amount",
                table: "sales_returns",
                type: "numeric(18,2)",
                precision: 18,
                scale: 2,
                nullable: false,
                defaultValue: 0m);

            // Backfill dữ liệu cũ: cộng dồn tax_amount từ các dòng đã lưu (đã tồn tại sẵn từ trước)
            // để chứng từ cũ cũng có total_tax_amount/grand_total đúng thay vì mặc định 0.
            migrationBuilder.Sql(@"
                UPDATE sales_returns sr
                SET total_tax_amount = COALESCE(line_totals.tax_sum, 0),
                    grand_total      = sr.total_payment + COALESCE(line_totals.tax_sum, 0)
                FROM (
                    SELECT sales_return_id, SUM(tax_amount) AS tax_sum
                    FROM sales_return_lines
                    GROUP BY sales_return_id
                ) AS line_totals
                WHERE sr.id = line_totals.sales_return_id;
            ");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "grand_total",
                table: "sales_returns");

            migrationBuilder.DropColumn(
                name: "total_tax_amount",
                table: "sales_returns");
        }
    }
}
