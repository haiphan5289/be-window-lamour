using System.Text.Json.Serialization;

namespace Lamour.Application.Features.Accounting.Dtos;

public class PaymentEntryDto
{
    [JsonPropertyName("id")]             public int     Id           { get; set; }
    [JsonPropertyName("description")]    public string  Description  { get; set; } = "";
    [JsonPropertyName("debit_account")]  public string  DebitAccount  { get; set; } = "";
    [JsonPropertyName("credit_account")] public string  CreditAccount { get; set; } = "";
    [JsonPropertyName("amount")]         public decimal Amount        { get; set; }
    [JsonPropertyName("subject_code")]   public string? SubjectCode   { get; set; }
    [JsonPropertyName("subject_name")]   public string? SubjectName   { get; set; }
    [JsonPropertyName("bank_account")]   public string? BankAccount   { get; set; }
}
