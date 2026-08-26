using System.Text.Json.Serialization;

namespace Lamour.Application.Features.Accounting.Dtos;

// 1 dòng trong popup "Thu tiền khách hàng hàng loạt" — 1 SalesOrder còn nợ.
public class OutstandingSalesOrderDto
{
    [JsonPropertyName("sales_order_id")]  public int      SalesOrderId   { get; set; }
    [JsonPropertyName("document_number")] public string   DocumentNumber { get; set; } = string.Empty;
    [JsonPropertyName("accounting_date")] public DateTime AccountingDate { get; set; }
    [JsonPropertyName("document_date")]   public DateTime DocumentDate   { get; set; }
    [JsonPropertyName("customer_id")]     public int      CustomerId     { get; set; }
    [JsonPropertyName("customer_code")]   public string   CustomerCode   { get; set; } = string.Empty;
    [JsonPropertyName("customer_name")]   public string   CustomerName   { get; set; } = string.Empty;
    [JsonPropertyName("description")]     public string?  Description    { get; set; }
    [JsonPropertyName("remaining_amount")] public decimal RemainingAmount { get; set; }
    // Bổ sung cho tab "2. Chứng từ" (so ảnh mẫu MISA) — lấy thẳng từ SalesOrder, không phải field mới.
    [JsonPropertyName("grand_total")]      public decimal  GrandTotal     { get; set; }
    [JsonPropertyName("payment_terms")]    public string?  PaymentTerms   { get; set; }
    [JsonPropertyName("payment_due_date")] public DateTime? PaymentDueDate { get; set; }
}

// 1 dòng người dùng tick chọn để thu tiền — SalesOrderId + số tiền thu (có thể < remaining_amount
// gốc nếu thu 1 phần).
public class BulkReceiptLineRequestDto
{
    [JsonPropertyName("sales_order_id")] public int     SalesOrderId { get; set; }
    [JsonPropertyName("amount")]         public decimal Amount       { get; set; }
}

public class CreateBulkCustomerReceiptRequestDto
{
    [JsonPropertyName("accounting_date")]       public DateTime AccountingDate      { get; set; }
    [JsonPropertyName("document_date")]         public DateTime DocumentDate        { get; set; }
    // "Cash111" (Tiền mặt) hoặc "Bank112" (Tiền gửi) — áp dụng chung cho toàn bộ phiếu (1 phiếu duy nhất).
    [JsonPropertyName("debit_account")]         public string   DebitAccount        { get; set; } = "Cash111";
    [JsonPropertyName("bank_account")]          public string?  BankAccount         { get; set; }
    [JsonPropertyName("collector_employee_id")] public int?     CollectorEmployeeId { get; set; }
    // "Người nộp" — khớp ảnh mẫu MISA: tên người/nhân viên nộp/thu tiền, KHÔNG phải tên 1 khách
    // hàng cụ thể (phiếu gộp nhiều khách hàng khác nhau). Rỗng → UseCase tự điền tên CollectorEmployee
    // hoặc "Thu tiền khách hàng hàng loạt".
    [JsonPropertyName("payer_name")]            public string?  PayerName           { get; set; }
    [JsonPropertyName("address")]               public string?  Address             { get; set; }
    [JsonPropertyName("attachment")]            public string?  Attachment          { get; set; }
    [JsonPropertyName("lines")]                 public List<BulkReceiptLineRequestDto> Lines { get; set; } = new();
}

// 1 phiếu thu duy nhất (khớp ảnh mẫu MISA — không còn group theo khách hàng ra nhiều phiếu).
public class CreateBulkCustomerReceiptResponseDto
{
    [JsonPropertyName("receipt")] public ReceiptResponseDto Receipt { get; set; } = new();
}
