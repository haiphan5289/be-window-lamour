using System.Text.Json.Serialization;

namespace Lamour.Application.Features.Deposits.Dtos;

public class CreateDepositDeductionRequestDto
{
    [JsonPropertyName("deposit_id")]       public int      DepositId      { get; set; }
    [JsonPropertyName("sales_order_id")]   public int      SalesOrderId   { get; set; }
    [JsonPropertyName("amount")]           public decimal  Amount         { get; set; }
    [JsonPropertyName("accounting_date")]  public DateTime AccountingDate { get; set; }
    [JsonPropertyName("document_date")]    public DateTime DocumentDate   { get; set; }
    [JsonPropertyName("description")]      public string?  Description    { get; set; }
}
