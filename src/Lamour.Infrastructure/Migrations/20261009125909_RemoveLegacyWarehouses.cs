using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Lamour.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class RemoveLegacyWarehouses : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            // Theo yêu cầu kế toán: chỉ có 2 kho thật là HH "Hàng hoá" và TB "Trưng bày". "Kho chính"
            // (KHO01, seed từ AddWarehouseReceipts) và "Kho chi nhánh Q.1" (KHO02, chèn thủ công ngoài
            // luồng) đã ngưng hoạt động từ DeactivateExtraWarehouses nhưng vẫn lên hộp tham số Tổng hợp
            // tồn kho. Gộp toàn bộ dữ liệu còn trỏ tới 2 kho này về HH rồi xoá hẳn — mọi FK tới
            // warehouses là Restrict nên phải chuyển dữ liệu trước khi xoá. Product.StockQuantity (tổng
            // các kho) không đổi vì tồn chỉ đổi kho, không đổi số lượng.
            migrationBuilder.Sql(@"
DO $$
DECLARE
    hh_id integer;
    legacy_ids integer[];
BEGIN
    SELECT array_agg(id) INTO legacy_ids FROM warehouses WHERE code IN ('KHO01', 'KHO02');
    IF legacy_ids IS NULL THEN
        RETURN;
    END IF;

    SELECT id INTO hh_id FROM warehouses WHERE code = 'HH';
    IF hh_id IS NULL THEN
        RAISE EXCEPTION 'Không tìm thấy kho HH để gộp dữ liệu từ KHO01/KHO02';
    END IF;

    INSERT INTO product_warehouse_stocks (product_id, warehouse_id, quantity)
    SELECT product_id, hh_id, SUM(quantity)
    FROM product_warehouse_stocks
    WHERE warehouse_id = ANY(legacy_ids)
    GROUP BY product_id
    ON CONFLICT (product_id, warehouse_id)
    DO UPDATE SET quantity = product_warehouse_stocks.quantity + EXCLUDED.quantity;

    DELETE FROM product_warehouse_stocks WHERE warehouse_id = ANY(legacy_ids);

    UPDATE warehouse_receipt_lines SET warehouse_id = hh_id WHERE warehouse_id = ANY(legacy_ids);
    UPDATE sales_order_lines       SET warehouse_id = hh_id WHERE warehouse_id = ANY(legacy_ids);
    UPDATE sales_return_lines      SET warehouse_id = hh_id WHERE warehouse_id = ANY(legacy_ids);
    UPDATE products SET default_warehouse_id = hh_id WHERE default_warehouse_id = ANY(legacy_ids);

    DELETE FROM warehouses WHERE code = 'KHO02';
END $$;
");

            migrationBuilder.DeleteData(
                table: "warehouses",
                keyColumn: "id",
                keyValue: 1);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            // Chỉ tạo lại dòng KHO01 — dữ liệu đã gộp về HH không tách ngược lại được.
            migrationBuilder.InsertData(
                table: "warehouses",
                columns: new[] { "id", "code", "is_active", "name" },
                values: new object[] { 1, "KHO01", true, "Kho chính" });
        }
    }
}
