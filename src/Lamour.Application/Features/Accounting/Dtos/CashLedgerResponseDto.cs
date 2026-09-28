using System.Text.Json.Serialization;

namespace Lamour.Application.Features.Accounting.Dtos;

public class CashLedgerResponseDto
{
    [JsonPropertyName("opening_balance")] public decimal OpeningBalance { get; set; }
    [JsonPropertyName("closing_balance")] public decimal ClosingBalance { get; set; }

    // 2026-09-28 (khớp box "Tồn quỹ đến hiện tại" ảnh mẫu MISA): tồn quỹ tiền mặt tính đến HÔM NAY,
    // độc lập với bộ lọc Từ ngày/Đến ngày đang chọn trên màn Quỹ — không phải ClosingBalance (vốn
    // chỉ tính tới `to` của filter).
    [JsonPropertyName("current_balance")] public decimal CurrentBalance { get; set; }

    [JsonPropertyName("entries")]         public List<CashLedgerEntryDto> Entries { get; set; } = new();
}
