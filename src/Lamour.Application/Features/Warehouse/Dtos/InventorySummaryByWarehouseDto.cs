using System.Text.Json.Serialization;

namespace Lamour.Application.Features.Warehouse.Dtos;

// Một nhóm "Tên kho : Hàng Hóa (N)" của báo cáo Tổng hợp tồn kho chia theo kho — Items mang số liệu
// RIÊNG của kho đó (cùng shape InventorySummaryItemDto của endpoint summary gộp).
public class InventorySummaryByWarehouseDto
{
    [JsonPropertyName("warehouse_id")]
    public int WarehouseId { get; set; }

    [JsonPropertyName("warehouse_code")]
    public string WarehouseCode { get; set; } = string.Empty;

    [JsonPropertyName("warehouse_name")]
    public string WarehouseName { get; set; } = string.Empty;

    [JsonPropertyName("items")]
    public List<InventorySummaryItemDto> Items { get; set; } = new();
}
