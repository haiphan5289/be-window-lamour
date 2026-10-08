using System.Text.Json.Serialization;

namespace Lamour.Application.Features.Warehouse.Dtos;

public class InventoryDetailLineDto
{
    [JsonPropertyName("accounting_date")]
    public DateTime AccountingDate { get; set; }

    [JsonPropertyName("document_date")]
    public DateTime DocumentDate { get; set; }

    [JsonPropertyName("document_number")]
    public string DocumentNumber { get; set; } = string.Empty;

    // "Import" | "Export" | "SalesReturn" — xem GetTransactionLinesByProductAsync.
    [JsonPropertyName("document_type")]
    public string DocumentType { get; set; } = string.Empty;

    // Id của WarehouseReceipt (Import) hoặc SalesOrder (Export) để client mở lại chứng từ gốc —
    // null cho SalesReturn (chưa có màn xem lại chứng từ trả hàng từ đây).
    [JsonPropertyName("source_id")]
    public int? SourceId { get; set; }

    [JsonPropertyName("description")]
    public string? Description { get; set; }

    [JsonPropertyName("unit")]
    public string Unit { get; set; } = string.Empty;

    [JsonPropertyName("import_qty")]
    public int ImportQty { get; set; }

    [JsonPropertyName("import_value")]
    public decimal ImportValue { get; set; }

    [JsonPropertyName("export_qty")]
    public int ExportQty { get; set; }

    [JsonPropertyName("export_value")]
    public decimal ExportValue { get; set; }

    // Kho của dòng (sổ chi tiết nhóm theo kho).
    [JsonPropertyName("warehouse_id")]
    public int WarehouseId { get; set; }

    [JsonPropertyName("warehouse_code")]
    public string WarehouseCode { get; set; } = string.Empty;

    [JsonPropertyName("warehouse_name")]
    public string WarehouseName { get; set; } = string.Empty;

    // Đơn giá = giá trị dòng / số lượng dòng (Nhập: ImportValue thật trên phiếu / ImportQty;
    // Xuất/Trả lại: Product.CostPrice); 0 khi số lượng dòng = 0.
    [JsonPropertyName("unit_price")]
    public decimal UnitPrice { get; set; }

    // Tồn chạy dần của CHÍNH KHO này sau khi áp dụng đúng dòng này (Opening của kho + cộng dồn Nhập − Xuất tính đến đây).
    [JsonPropertyName("running_qty")]
    public int RunningQty { get; set; }

    [JsonPropertyName("running_value")]
    public decimal RunningValue { get; set; }
}

// Tồn đầu/cuối của một kho trong sổ chi tiết của một sản phẩm.
public class InventoryDetailWarehouseDto
{
    [JsonPropertyName("warehouse_id")]
    public int WarehouseId { get; set; }

    [JsonPropertyName("warehouse_code")]
    public string WarehouseCode { get; set; } = string.Empty;

    [JsonPropertyName("warehouse_name")]
    public string WarehouseName { get; set; } = string.Empty;

    [JsonPropertyName("opening_qty")]
    public int OpeningQty { get; set; }

    [JsonPropertyName("opening_value")]
    public decimal OpeningValue { get; set; }

    [JsonPropertyName("closing_qty")]
    public int ClosingQty { get; set; }

    [JsonPropertyName("closing_value")]
    public decimal ClosingValue { get; set; }
}

public class InventoryDetailResponseDto
{
    [JsonPropertyName("product_id")]
    public int ProductId { get; set; }

    [JsonPropertyName("code")]
    public string Code { get; set; } = string.Empty;

    [JsonPropertyName("name")]
    public string Name { get; set; } = string.Empty;

    [JsonPropertyName("unit")]
    public string Unit { get; set; } = string.Empty;

    [JsonPropertyName("opening_qty")]
    public int OpeningQty { get; set; }

    [JsonPropertyName("opening_value")]
    public decimal OpeningValue { get; set; }

    [JsonPropertyName("closing_qty")]
    public int ClosingQty { get; set; }

    [JsonPropertyName("closing_value")]
    public decimal ClosingValue { get; set; }

    // Tồn đầu/cuối theo từng kho liên quan (sắp theo tên kho). opening_/closing_ ở trên = tổng các kho này.
    [JsonPropertyName("warehouses")]
    public List<InventoryDetailWarehouseDto> Warehouses { get; set; } = new();

    [JsonPropertyName("lines")]
    public List<InventoryDetailLineDto> Lines { get; set; } = new();
}
