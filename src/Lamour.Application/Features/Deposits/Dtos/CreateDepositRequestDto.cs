using System.Text.Json.Serialization;

namespace Lamour.Application.Features.Deposits.Dtos;

public class CreateDepositRequestDto
{
    [JsonPropertyName("document_number")]  public string   DocumentNumber { get; set; } = "";
    [JsonPropertyName("accounting_date")]  public DateTime AccountingDate { get; set; }
    [JsonPropertyName("document_date")]    public DateTime DocumentDate   { get; set; }
    [JsonPropertyName("customer_id")]      public int      CustomerId     { get; set; }
    [JsonPropertyName("employee_id")]      public int?     EmployeeId     { get; set; }
    [JsonPropertyName("description")]      public string?  Description    { get; set; }
    [JsonPropertyName("reference")]        public string?  Reference      { get; set; }
    [JsonPropertyName("amount")]           public decimal  Amount         { get; set; }
}
