using System.Text.Json.Serialization;

namespace Lamour.Application.Features.Accounting.Dtos;

public class CreateReceiptRequestDto
{
    // "Customer" | "Employee". Bỏ trống cả partner_type lẫn partner_id = phiếu thu hàng loạt (chỉ hợp lệ
    // với payment_reason = ThuKhachHangHangLoat).
    [JsonPropertyName("partner_type")]          public string?  PartnerType         { get; set; }
    [JsonPropertyName("partner_id")]            public int?     PartnerId           { get; set; }
    [JsonPropertyName("payer_name")]            public string   PayerName           { get; set; } = "";
    [JsonPropertyName("address")]               public string?  Address             { get; set; }
    [JsonPropertyName("payment_reason")]        public string   PaymentReason       { get; set; } = "ThuKhac";
    [JsonPropertyName("reason_detail")]         public string?  ReasonDetail        { get; set; }
    [JsonPropertyName("collector_employee_id")] public int?     CollectorEmployeeId { get; set; }
    [JsonPropertyName("attachment")]            public string?  Attachment          { get; set; }
    [JsonPropertyName("reference")]             public string?  Reference           { get; set; }
    [JsonPropertyName("accounting_date")]       public DateTime AccountingDate      { get; set; }
    [JsonPropertyName("document_date")]         public DateTime DocumentDate        { get; set; }
    [JsonPropertyName("document_number")]       public string   DocumentNumber      { get; set; } = "";
    [JsonPropertyName("entries")]               public List<ReceiptEntryDto> Entries { get; set; } = new();
}
