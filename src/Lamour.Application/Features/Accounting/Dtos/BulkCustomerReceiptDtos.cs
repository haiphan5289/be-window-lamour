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
    // "Cash111" (Tiền mặt) hoặc "Bank112" (Tiền gửi) — áp dụng chung cho mọi phiếu được tạo trong 1 lần thu.
    [JsonPropertyName("debit_account")]         public string   DebitAccount        { get; set; } = "Cash111";
    [JsonPropertyName("bank_account")]          public string?  BankAccount         { get; set; }
    [JsonPropertyName("collector_employee_id")] public int?     CollectorEmployeeId { get; set; }
    [JsonPropertyName("lines")]                 public List<BulkReceiptLineRequestDto> Lines { get; set; } = new();
}

public class CreateBulkCustomerReceiptResponseDto
{
    [JsonPropertyName("receipts")] public List<ReceiptResponseDto> Receipts { get; set; } = new();
}
