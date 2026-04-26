using System.Text.Json.Serialization;

namespace Lamour.Application.Features.Accounting.Dtos;

public class PaymentReceiptLineDto
{
    [JsonPropertyName("id")]              public int       Id             { get; set; }
    [JsonPropertyName("document_date")]   public DateTime  DocumentDate   { get; set; }
    [JsonPropertyName("document_number")] public string    DocumentNumber  { get; set; } = "";
    [JsonPropertyName("invoice_number")]  public string    InvoiceNumber   { get; set; } = "";
    [JsonPropertyName("description")]     public string    Description     { get; set; } = "";
    [JsonPropertyName("due_date")]        public DateTime? DueDate         { get; set; }
    [JsonPropertyName("amount_due")]      public decimal   AmountDue       { get; set; }
    [JsonPropertyName("amount_paid")]     public decimal   AmountPaid      { get; set; }
}
