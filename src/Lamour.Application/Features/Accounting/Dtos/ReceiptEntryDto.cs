using System.Text.Json.Serialization;

namespace Lamour.Application.Features.Accounting.Dtos;

public class ReceiptEntryDto
{
    [JsonPropertyName("id")]             public int     Id           { get; set; }
    [JsonPropertyName("description")]    public string  Description  { get; set; } = "";
    [JsonPropertyName("debit_account_id")]           public int     DebitAccountId           { get; set; }
    [JsonPropertyName("debit_account_code")]         public string? DebitAccountCode         { get; set; }
    [JsonPropertyName("debit_account_description")]  public string? DebitAccountDescription  { get; set; }
    [JsonPropertyName("credit_account_id")]          public int     CreditAccountId          { get; set; }
    [JsonPropertyName("credit_account_code")]        public string? CreditAccountCode        { get; set; }
    [JsonPropertyName("credit_account_description")] public string? CreditAccountDescription { get; set; }
    // Chỉ dùng ở REQUEST của Phiếu thu hàng loạt (màn đó chọn "Tiền mặt / Tiền gửi", không có danh mục
    // TK): tên cũ "Cash111" | "Bank112" | "Receivable131" | "Payroll334", BE tự tra sang danh mục khi
    // *_account_id = 0. Response không trả 2 field này.
    [JsonPropertyName("debit_account")]  public string? DebitAccount  { get; set; }
    [JsonPropertyName("credit_account")] public string? CreditAccount { get; set; }
    [JsonPropertyName("amount")]         public decimal Amount        { get; set; }
    [JsonPropertyName("subject_code")]   public string? SubjectCode   { get; set; }
    [JsonPropertyName("subject_name")]   public string? SubjectName   { get; set; }
    [JsonPropertyName("bank_account")]   public string? BankAccount   { get; set; }
    [JsonPropertyName("sales_order_id")] public int?    SalesOrderId  { get; set; }
}
