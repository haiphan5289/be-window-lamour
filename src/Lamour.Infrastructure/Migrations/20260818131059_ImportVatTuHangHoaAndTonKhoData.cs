using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Lamour.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class ImportVatTuHangHoaAndTonKhoData : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            // Nguồn: Downloads/"vật tư hàng hóa.xlsx" (72 dòng, Mã/Tên/Tính chất/ĐVT/Số lượng tồn tối thiểu/Giảm thuế theo QĐ)
            // + Downloads/"Tổng hợp tồn kho.xlsx" (95 dòng tồn kho theo 2 kho HH/TB, ngày 2026-08-18).
            // Upsert theo `code` — an toàn chạy lại nhiều lần (ON CONFLICT DO UPDATE).
            migrationBuilder.Sql(@"
INSERT INTO products (code, name, unit, product_unit_id, nature, min_stock_quantity, tax_reduction_type, is_deposit_product, stock_quantity, cost_price, selling_price, category_id)
SELECT v.code, v.name, v.unit, v.product_unit_id, v.nature, v.min_stock_quantity, v.tax_reduction_type, v.is_deposit_product, v.stock_quantity, 0, 0, NULL
FROM (VALUES
    ('1', 'Biocell Face Scrub', 'Chai', (SELECT id FROM product_units WHERE name = 'Chai'), 'VatTuHangHoa', 0, 'CoGiamThue', false, 219),
    ('10', 'Meso Calming Ampoule Mask II 10M', 'Hộp', (SELECT id FROM product_units WHERE name = 'Hộp'), 'VatTuHangHoa', 0, 'ChuaXacDinh', false, 1),
    ('11', 'Mesotox Skin Booster V-Plot', 'Hộp', (SELECT id FROM product_units WHERE name = 'Hộp'), 'VatTuHangHoa', 0, 'ChuaXacDinh', false, 107),
    ('12', 'EXO LLT Pro TRI Fills Solution', 'Hộp', (SELECT id FROM product_units WHERE name = 'Hộp'), 'VatTuHangHoa', 0, 'CoGiamThue', false, 221),
    ('13', 'EXO LLT Pro AC Fills Solution', 'Hộp', (SELECT id FROM product_units WHERE name = 'Hộp'), 'VatTuHangHoa', 0, 'CoGiamThue', false, 207),
    ('14', 'Meso Filler', 'Cái', (SELECT id FROM product_units WHERE name = 'Cái'), 'VatTuHangHoa', 0, 'ChuaXacDinh', false, 13),
    ('15', 'Meso Fills', 'Hộp', (SELECT id FROM product_units WHERE name = 'Hộp'), 'VatTuHangHoa', 0, 'ChuaXacDinh', false, 196),
    ('16', 'Meso Hydro Ampoule Mask', 'Hộp', (SELECT id FROM product_units WHERE name = 'Hộp'), 'VatTuHangHoa', 0, 'CoGiamThue', false, 166),
    ('17', 'Meso Hydro Mask', 'Hộp', (SELECT id FROM product_units WHERE name = 'Hộp'), 'VatTuHangHoa', 0, 'ChuaXacDinh', false, 0),
    ('18', 'Numa Cream', 'Tuýp', (SELECT id FROM product_units WHERE name = 'Tuýp'), 'VatTuHangHoa', 0, 'ChuaXacDinh', false, 106),
    ('19', 'Ống Xillanh', 'Cái', (SELECT id FROM product_units WHERE name = 'Cái'), 'VatTuHangHoa', 0, 'ChuaXacDinh', false, 328),
    ('2', 'Antioxidant Cream Mask', 'Hộp', (SELECT id FROM product_units WHERE name = 'Hộp'), 'VatTuHangHoa', 0, 'CoGiamThue', false, 120),
    ('20', 'PH Balance Cleansing Lotion', 'Chai', (SELECT id FROM product_units WHERE name = 'Chai'), 'VatTuHangHoa', 0, 'CoGiamThue', false, 27),
    ('21', 'Soothing Massage Cream', 'Chai', (SELECT id FROM product_units WHERE name = 'Chai'), 'VatTuHangHoa', 0, 'CoGiamThue', false, 21),
    ('22', 'PH Balance Toning Lotion', 'Chai', (SELECT id FROM product_units WHERE name = 'Chai'), 'VatTuHangHoa', 0, 'CoGiamThue', false, 33),
    ('23', 'Skin Cooler', 'Hộp', (SELECT id FROM product_units WHERE name = 'Hộp'), 'VatTuHangHoa', 0, 'ChuaXacDinh', false, 157),
    ('24', 'UV Block', 'Hộp', (SELECT id FROM product_units WHERE name = 'Hộp'), 'VatTuHangHoa', 0, 'CoGiamThue', false, 1273),
    ('25', 'BB Cream', 'Tuýp', (SELECT id FROM product_units WHERE name = 'Tuýp'), 'VatTuHangHoa', 0, 'CoGiamThue', false, 259),
    ('26', 'Ves Serum', 'Hộp', (SELECT id FROM product_units WHERE name = 'Hộp'), 'VatTuHangHoa', 0, 'CoGiamThue', false, 1439),
    ('27', 'Bubble Cleanser', 'Chai', (SELECT id FROM product_units WHERE name = 'Chai'), 'VatTuHangHoa', 0, 'CoGiamThue', false, 154),
    ('28', 'Centella TC Cream', 'Tuýp', (SELECT id FROM product_units WHERE name = 'Tuýp'), 'VatTuHangHoa', 0, 'CoGiamThue', false, 1201),
    ('29', 'Meso Calming Ampoule Mask II', 'Hộp', (SELECT id FROM product_units WHERE name = 'Hộp'), 'VatTuHangHoa', 0, 'CoGiamThue', false, 2878),
    ('3', 'Antioxidant Serum', 'Hộp', (SELECT id FROM product_units WHERE name = 'Hộp'), 'VatTuHangHoa', 0, 'CoGiamThue', false, 143),
    ('30', 'Skin Hydration Cleansing Gel', 'Chai', (SELECT id FROM product_units WHERE name = 'Chai'), 'VatTuHangHoa', 0, 'CoGiamThue', false, 68),
    ('31', 'Skin Hydration Gel Toner', 'Chai', (SELECT id FROM product_units WHERE name = 'Chai'), 'VatTuHangHoa', 0, 'CoGiamThue', false, 403),
    ('32', 'PH Balance Cleansing Cream', 'Chai', (SELECT id FROM product_units WHERE name = 'Chai'), 'VatTuHangHoa', 0, 'CoGiamThue', false, 230),
    ('33', 'Centella Calmimg Gel Cream', 'Hộp', (SELECT id FROM product_units WHERE name = 'Hộp'), 'VatTuHangHoa', 0, 'CoGiamThue', false, 26),
    ('34', 'Complex AC Ampoule', 'Hộp', (SELECT id FROM product_units WHERE name = 'Hộp'), 'VatTuHangHoa', 0, 'CoGiamThue', false, 53),
    ('35', 'Complex AC Cleanser', 'Chai', (SELECT id FROM product_units WHERE name = 'Chai'), 'VatTuHangHoa', 0, 'CoGiamThue', false, 83),
    ('36', 'Trừ Cọc', '', NULL, 'VatTuHangHoa', 0, 'ChuaXacDinh', true, 0),
    ('37', 'Đầu Kim', 'Cái', (SELECT id FROM product_units WHERE name = 'Cái'), 'VatTuHangHoa', 0, 'ChuaXacDinh', false, 367),
    ('38', 'Meso C Cream', 'Hộp', (SELECT id FROM product_units WHERE name = 'Hộp'), 'VatTuHangHoa', 0, 'ChuaXacDinh', false, 72),
    ('39', 'Brightening Fills Ampoule', 'Hộp', (SELECT id FROM product_units WHERE name = 'Hộp'), 'VatTuHangHoa', 0, 'CoGiamThue', false, 130),
    ('4', 'SÁCH 33 NHÂN HIỆU CHỦ SPA THÀNH CÔNG', 'Cuốn', (SELECT id FROM product_units WHERE name = 'Cuốn'), 'VatTuHangHoa', 0, 'ChuaXacDinh', false, 1),
    ('40', 'Meso Fills Cream', 'Hộp', (SELECT id FROM product_units WHERE name = 'Hộp'), 'VatTuHangHoa', 0, 'ChuaXacDinh', false, 76),
    ('41', 'Wrinkle Care Eye Cream', 'Hộp', (SELECT id FROM product_units WHERE name = 'Hộp'), 'VatTuHangHoa', 0, 'CoGiamThue', false, 259),
    ('42', 'Anti Wrinkle Eye Mask', 'Hộp', (SELECT id FROM product_units WHERE name = 'Hộp'), 'VatTuHangHoa', 0, 'CoGiamThue', false, 96),
    ('43', 'Ethosome Astaxanthin', 'Hộp', (SELECT id FROM product_units WHERE name = 'Hộp'), 'VatTuHangHoa', 0, 'CoGiamThue', false, 1378),
    ('44', 'E.G.F Stem C', 'Hộp', (SELECT id FROM product_units WHERE name = 'Hộp'), 'VatTuHangHoa', 0, 'CoGiamThue', false, 667),
    ('45', 'Meso Hydro Mask (100ml)', 'Hộp', (SELECT id FROM product_units WHERE name = 'Hộp'), 'VatTuHangHoa', 0, 'ChuaXacDinh', false, 175),
    ('46', 'Multi- Vitamin B', 'Chai', (SELECT id FROM product_units WHERE name = 'Chai'), 'VatTuHangHoa', 0, 'CoGiamThue', false, 103),
    ('47', 'Blue Energy', 'Chai', (SELECT id FROM product_units WHERE name = 'Chai'), 'VatTuHangHoa', 0, 'CoGiamThue', false, 172),
    ('48', 'Time Reset', 'Chai', (SELECT id FROM product_units WHERE name = 'Chai'), 'VatTuHangHoa', 0, 'CoGiamThue', false, 315),
    ('49', 'Multi- Vitamin C', 'Chai', (SELECT id FROM product_units WHERE name = 'Chai'), 'VatTuHangHoa', 0, 'CoGiamThue', false, 211),
    ('5', 'Meso Filler Pro', 'Bộ', (SELECT id FROM product_units WHERE name = 'Bộ'), 'VatTuHangHoa', 0, 'ChuaXacDinh', false, 35),
    ('50', 'Pink Energy', 'Chai', (SELECT id FROM product_units WHERE name = 'Chai'), 'VatTuHangHoa', 0, 'CoGiamThue', false, 120),
    ('51', 'Skin Brightening ACT', 'Chai', (SELECT id FROM product_units WHERE name = 'Chai'), 'VatTuHangHoa', 0, 'CoGiamThue', false, 55),
    ('52', 'Skin Boosting ACT', 'Chai', (SELECT id FROM product_units WHERE name = 'Chai'), 'VatTuHangHoa', 0, 'CoGiamThue', false, 163),
    ('53', 'Mesotox Lipo Cocktail', 'Hộp', (SELECT id FROM product_units WHERE name = 'Hộp'), 'VatTuHangHoa', 0, 'ChuaXacDinh', false, 54),
    ('54', 'Mesotox Skin Booster Glutathion', 'Hộp', (SELECT id FROM product_units WHERE name = 'Hộp'), 'VatTuHangHoa', 0, 'ChuaXacDinh', false, 93),
    ('55', 'Mesotox Skin Booster PDRN', 'Hộp', (SELECT id FROM product_units WHERE name = 'Hộp'), 'VatTuHangHoa', 0, 'ChuaXacDinh', false, 106),
    ('56', 'Meso Peel CL4 100ml', 'Chai', (SELECT id FROM product_units WHERE name = 'Chai'), 'VatTuHangHoa', 0, 'CoGiamThue', false, 163),
    ('57', 'Clear Clarifying Cream Mask', 'Hộp', (SELECT id FROM product_units WHERE name = 'Hộp'), 'VatTuHangHoa', 0, 'CoGiamThue', false, 246),
    ('58', 'Vita Radiance Cream Mask', 'Hộp', (SELECT id FROM product_units WHERE name = 'Hộp'), 'VatTuHangHoa', 0, 'CoGiamThue', false, 62),
    ('59', 'Vita Radiance Serum', 'Hộp', (SELECT id FROM product_units WHERE name = 'Hộp'), 'VatTuHangHoa', 0, 'CoGiamThue', false, 65),
    ('6', 'Ống Xilanh Pro', 'Cái', (SELECT id FROM product_units WHERE name = 'Cái'), 'VatTuHangHoa', 0, 'ChuaXacDinh', false, 135),
    ('60', 'Centella Soothing Serum', 'Hộp', (SELECT id FROM product_units WHERE name = 'Hộp'), 'VatTuHangHoa', 0, 'CoGiamThue', false, 182),
    ('61', 'Clear Clarifying Serum', 'Hộp', (SELECT id FROM product_units WHERE name = 'Hộp'), 'VatTuHangHoa', 0, 'CoGiamThue', false, 139),
    ('62', 'B5 Moisturizing Cream Mask', 'Hộp', (SELECT id FROM product_units WHERE name = 'Hộp'), 'VatTuHangHoa', 0, 'CoGiamThue', false, 101),
    ('63', 'B5 Moisturizing Serum', 'Hộp', (SELECT id FROM product_units WHERE name = 'Hộp'), 'VatTuHangHoa', 0, 'CoGiamThue', false, 155),
    ('64', 'Centella Cream Mask', 'Hộp', (SELECT id FROM product_units WHERE name = 'Hộp'), 'VatTuHangHoa', 0, 'CoGiamThue', false, 99),
    ('65', 'CỌC', '', NULL, 'VatTuHangHoa', 0, 'ChuaXacDinh', true, 0),
    ('66', 'Radiance Lha Peel Pad', 'Hộp', (SELECT id FROM product_units WHERE name = 'Hộp'), 'VatTuHangHoa', 0, 'CoGiamThue', false, 145),
    ('67', 'Azulene Care Mist', 'Chai', (SELECT id FROM product_units WHERE name = 'Chai'), 'VatTuHangHoa', 0, 'CoGiamThue', false, 188),
    ('68', 'Honeybush Skinsolution', 'Hộp', (SELECT id FROM product_units WHERE name = 'Hộp'), 'VatTuHangHoa', 0, 'ChuaXacDinh', false, 519),
    ('69', 'DERMA MATRIX EXO-PN', 'Hộp', (SELECT id FROM product_units WHERE name = 'Hộp'), 'VatTuHangHoa', 0, 'ChuaXacDinh', false, 280),
    ('7', 'Nút Cao Su', 'Cái', (SELECT id FROM product_units WHERE name = 'Cái'), 'VatTuHangHoa', 0, 'ChuaXacDinh', false, 398),
    ('8', 'Đầu Kim Pro', 'Cái', (SELECT id FROM product_units WHERE name = 'Cái'), 'VatTuHangHoa', 0, 'ChuaXacDinh', false, 71),
    ('9', 'Mesotox Skin Booster Scalp', 'Hộp', (SELECT id FROM product_units WHERE name = 'Hộp'), 'VatTuHangHoa', 0, 'ChuaXacDinh', false, 106),
    ('CPMH', 'Chi phí mua hàng', '', NULL, 'DichVu', 0, 'ChuaXacDinh', false, 0),
    ('KHACHSAN_PHI_PHUCVU', 'Phí phục vụ', '', NULL, 'DichVu', 0, 'ChuaXacDinh', false, 0),
    ('LPXD', 'Lệ phí xăng dầu', '', NULL, 'DichVu', 0, 'ChuaXacDinh', false, 0)
) AS v(code, name, unit, product_unit_id, nature, min_stock_quantity, tax_reduction_type, is_deposit_product, stock_quantity)
ON CONFLICT (code) WHERE code <> '' DO UPDATE SET
    name = EXCLUDED.name,
    unit = EXCLUDED.unit,
    product_unit_id = EXCLUDED.product_unit_id,
    nature = EXCLUDED.nature,
    min_stock_quantity = EXCLUDED.min_stock_quantity,
    tax_reduction_type = EXCLUDED.tax_reduction_type,
    is_deposit_product = EXCLUDED.is_deposit_product,
    stock_quantity = EXCLUDED.stock_quantity;
");

            // Tồn kho theo từng kho — join theo product.code + warehouse.code (HH/TB đã seed sẵn từ migration ExtendProductForVTHHForm).
            migrationBuilder.Sql(@"
INSERT INTO product_warehouse_stocks (product_id, warehouse_id, quantity)
SELECT p.id, w.id, v.quantity
FROM (VALUES
    ('1', 'HH', 216),
    ('1', 'TB', 3),
    ('10', 'HH', 1),
    ('11', 'HH', 107),
    ('12', 'HH', 221),
    ('13', 'HH', 207),
    ('14', 'HH', 13),
    ('15', 'HH', 193),
    ('15', 'TB', 3),
    ('16', 'HH', 163),
    ('16', 'TB', 3),
    ('18', 'HH', 106),
    ('19', 'HH', 318),
    ('19', 'TB', 10),
    ('2', 'HH', 120),
    ('20', 'HH', 24),
    ('20', 'TB', 3),
    ('21', 'HH', 18),
    ('21', 'TB', 3),
    ('22', 'HH', 30),
    ('22', 'TB', 3),
    ('23', 'HH', 152),
    ('23', 'TB', 5),
    ('24', 'HH', 1270),
    ('24', 'TB', 3),
    ('25', 'HH', 256),
    ('25', 'TB', 3),
    ('26', 'HH', 1436),
    ('26', 'TB', 3),
    ('27', 'HH', 151),
    ('27', 'TB', 3),
    ('28', 'HH', 1198),
    ('28', 'TB', 3),
    ('29', 'HH', 2878),
    ('3', 'HH', 143),
    ('30', 'HH', 65),
    ('30', 'TB', 3),
    ('31', 'HH', 400),
    ('31', 'TB', 3),
    ('32', 'HH', 227),
    ('32', 'TB', 3),
    ('33', 'HH', 23),
    ('33', 'TB', 3),
    ('34', 'HH', 50),
    ('34', 'TB', 3),
    ('35', 'HH', 80),
    ('35', 'TB', 3),
    ('37', 'HH', 357),
    ('37', 'TB', 10),
    ('38', 'HH', 69),
    ('38', 'TB', 3),
    ('39', 'HH', 127),
    ('39', 'TB', 3),
    ('4', 'HH', 1),
    ('40', 'HH', 73),
    ('40', 'TB', 3),
    ('41', 'HH', 256),
    ('41', 'TB', 3),
    ('42', 'HH', 93),
    ('42', 'TB', 3),
    ('43', 'HH', 1375),
    ('43', 'TB', 3),
    ('44', 'HH', 664),
    ('44', 'TB', 3),
    ('45', 'HH', 172),
    ('45', 'TB', 3),
    ('46', 'HH', 103),
    ('47', 'HH', 172),
    ('48', 'HH', 315),
    ('49', 'HH', 211),
    ('5', 'HH', 35),
    ('50', 'HH', 120),
    ('51', 'HH', 55),
    ('52', 'HH', 163),
    ('53', 'HH', 54),
    ('54', 'HH', 93),
    ('55', 'HH', 106),
    ('56', 'HH', 160),
    ('56', 'TB', 3),
    ('57', 'HH', 246),
    ('58', 'HH', 62),
    ('59', 'HH', 65),
    ('6', 'HH', 135),
    ('60', 'HH', 182),
    ('61', 'HH', 139),
    ('62', 'HH', 101),
    ('63', 'HH', 155),
    ('64', 'HH', 99),
    ('66', 'HH', 145),
    ('67', 'HH', 188),
    ('68', 'HH', 519),
    ('69', 'HH', 280),
    ('7', 'HH', 398),
    ('8', 'HH', 71),
    ('9', 'HH', 106)
) AS v(product_code, warehouse_code, quantity)
JOIN products p ON p.code = v.product_code
JOIN warehouses w ON w.code = v.warehouse_code
ON CONFLICT (product_id, warehouse_id) DO UPDATE SET
    quantity = EXCLUDED.quantity;
");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            // product_warehouse_stocks tự xóa theo (ON DELETE CASCADE trên product_id).
            migrationBuilder.Sql(@"
DELETE FROM products WHERE code IN ('1', '10', '11', '12', '13', '14', '15', '16', '17', '18', '19', '2', '20', '21', '22', '23', '24', '25', '26', '27', '28', '29', '3', '30', '31', '32', '33', '34', '35', '36', '37', '38', '39', '4', '40', '41', '42', '43', '44', '45', '46', '47', '48', '49', '5', '50', '51', '52', '53', '54', '55', '56', '57', '58', '59', '6', '60', '61', '62', '63', '64', '65', '66', '67', '68', '69', '7', '8', '9', 'CPMH', 'KHACHSAN_PHI_PHUCVU', 'LPXD');
");
        }
    }
}
