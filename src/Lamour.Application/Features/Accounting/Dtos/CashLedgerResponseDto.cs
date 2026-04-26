using System.Text.Json.Serialization;

namespace Lamour.Application.Features.Accounting.Dtos;

public class CashLedgerResponseDto
{
    [JsonPropertyName("opening_balance")] public decimal OpeningBalance { get; set; }
    [JsonPropertyName("closing_balance")] public decimal ClosingBalance { get; set; }
    [JsonPropertyName("entries")]         public List<CashLedgerEntryDto> Entries { get; set; } = new();
}
