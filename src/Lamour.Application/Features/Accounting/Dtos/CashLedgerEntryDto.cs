using System.Text.Json.Serialization;

namespace Lamour.Application.Features.Accounting.Dtos;

public class CashLedgerEntryDto
{
    [JsonPropertyName("accounting_date")] public DateTime AccountingDate { get; set; }
    [JsonPropertyName("document_date")]   public DateTime DocumentDate { get; set; }
    [JsonPropertyName("receipt_number")]  public string? ReceiptNumber { get; set; }
    [JsonPropertyName("payment_number")]  public string? PaymentNumber { get; set; }
    [JsonPropertyName("description")]     public string Description { get; set; } = "";
    [JsonPropertyName("account")]         public string Account { get; set; } = "";
    [JsonPropertyName("counter_account")] public string CounterAccount { get; set; } = "";
    [JsonPropertyName("debit_amount")]    public decimal DebitAmount { get; set; }
    [JsonPropertyName("credit_amount")]   public decimal CreditAmount { get; set; }
    [JsonPropertyName("amount")]          public decimal Amount { get; set; }
    [JsonPropertyName("balance")]         public decimal Balance { get; set; }
    [JsonPropertyName("person_name")]     public string? PersonName { get; set; }
    [JsonPropertyName("payment_reason")]  public string? PaymentReason { get; set; }
    [JsonPropertyName("document_type")]   public string DocumentType { get; set; } = "";
    [JsonPropertyName("status")]          public string Status { get; set; } = "Confirmed";

    // 2026-09-26: id phiếu gốc để WPF (màn Quỹ) gọi thẳng Ghi sổ / Bỏ ghi / Xóa / Sửa trên dòng đang
    // chọn — CashTransaction chỉ lưu số chứng từ, nên id được tra lại theo số phiếu. Null nếu dòng
    // không tìm thấy phiếu gốc (vd. phiếu đã bị xoá nhưng giao dịch quỹ cũ còn lại).
    [JsonPropertyName("receipt_id")]      public int? ReceiptId { get; set; }
    [JsonPropertyName("payment_id")]      public int? PaymentId { get; set; }

    // 2026-09-26: true = phiếu thu HÀNG LOẠT (Receipt.CustomerId == null) — WPF mở BulkCustomerReceiptWindow
    // thay vì ReceiptWindow khi double-click/Sửa (trước đây mở nhầm, Cất báo "Vui lòng chọn đối tượng").
    [JsonPropertyName("is_bulk_receipt")] public bool IsBulkReceipt { get; set; }
}
