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

    // Tồn chạy dần sau khi áp dụng đúng dòng này (Opening + cộng dồn Nhập − Xuất tính đến đây).
    [JsonPropertyName("running_qty")]
    public int RunningQty { get; set; }

    [JsonPropertyName("running_value")]
    public decimal RunningValue { get; set; }
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

    [JsonPropertyName("lines")]
    public List<InventoryDetailLineDto> Lines { get; set; } = new();
}
