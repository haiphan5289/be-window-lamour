using System.Text.Json.Serialization;

namespace Lamour.Application.Features.Accounting.Dtos;

public class CreatePaymentReceiptRequestDto
{
    [JsonPropertyName("customer_id")]     public int                             CustomerId     { get; set; }
    [JsonPropertyName("employee_id")]     public int?                            EmployeeId     { get; set; }
    [JsonPropertyName("collection_date")] public DateTime                        CollectionDate { get; set; }
    [JsonPropertyName("description")]     public string?                         Description    { get; set; }
    [JsonPropertyName("total_amount")]    public decimal                         TotalAmount    { get; set; }
    [JsonPropertyName("payment_method")] public string                          PaymentMethod  { get; set; } = "Cash";
    [JsonPropertyName("currency")]        public string                          Currency       { get; set; } = "VND";
    [JsonPropertyName("exchange_rate")]   public decimal                         ExchangeRate   { get; set; } = 1m;
    [JsonPropertyName("lines")]           public List<CreatePaymentReceiptLineDto> Lines        { get; set; } = new();
}
