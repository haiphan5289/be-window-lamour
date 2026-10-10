using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Lamour.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddSalesOrderExportNumber : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "export_number",
                table: "sales_orders",
                type: "character varying(50)",
                maxLength: 50,
                nullable: true);

            // Backfill TRƯỚC khi tạo unique index: đơn tạo từ Kho (đã mang số XK) giữ nguyên số đó; đơn BH được cấp số XK
            // kế tiếp sau số XK lớn nhất hiện có, theo thứ tự ngày hạch toán rồi id (cũ trước, mới sau). Chạy lại an toàn
            // (chỉ đụng dòng export_number IS NULL).
            migrationBuilder.Sql(@"
UPDATE sales_orders SET export_number = document_number
WHERE export_number IS NULL AND document_number LIKE 'XK%';

WITH base AS (
    SELECT COALESCE(MAX(NULLIF(regexp_replace(export_number, '\D', '', 'g'), '')::int), 0) AS max_no
    FROM sales_orders WHERE export_number LIKE 'XK%'
),
ranked AS (
    SELECT o.id, ROW_NUMBER() OVER (ORDER BY o.accounting_date, o.id) AS rn
    FROM sales_orders o WHERE o.export_number IS NULL
)
UPDATE sales_orders o
SET export_number = 'XK' || lpad((base.max_no + ranked.rn)::text, 5, '0')
FROM ranked, base
WHERE o.id = ranked.id;
");

            migrationBuilder.CreateIndex(
                name: "IX_sales_orders_export_number",
                table: "sales_orders",
                column: "export_number",
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_sales_orders_export_number",
                table: "sales_orders");

            migrationBuilder.DropColumn(
                name: "export_number",
                table: "sales_orders");
        }
    }
}
