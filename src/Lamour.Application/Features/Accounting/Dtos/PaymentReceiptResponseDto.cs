using System.Text.Json.Serialization;

namespace Lamour.Application.Features.Accounting.Dtos;

public class PaymentReceiptResponseDto
{
    [JsonPropertyName("id")]               public int                       Id            { get; set; }
    [JsonPropertyName("receipt_number")]   public string                    ReceiptNumber { get; set; } = "";
    [JsonPropertyName("customer_id")]      public int                       CustomerId    { get; set; }
    [JsonPropertyName("customer_name")]    public string                    CustomerName  { get; set; } = "";
    [JsonPropertyName("employee_id")]      public int?                      EmployeeId    { get; set; }
    [JsonPropertyName("employee_name")]    public string?                   EmployeeName  { get; set; }
    [JsonPropertyName("collection_date")]  public DateTime                  CollectionDate { get; set; }
    [JsonPropertyName("total_amount")]     public decimal                   TotalAmount   { get; set; }
    [JsonPropertyName("payment_method")]   public string                    PaymentMethod { get; set; } = "";
    [JsonPropertyName("currency")]         public string                    Currency      { get; set; } = "";
    [JsonPropertyName("exchange_rate")]    public decimal                   ExchangeRate  { get; set; }
    [JsonPropertyName("created_at")]       public DateTime                  CreatedAt     { get; set; }
    [JsonPropertyName("lines")]            public List<PaymentReceiptLineDto> Lines       { get; set; } = new();
}
