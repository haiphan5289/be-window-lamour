namespace Lamour.Domain.Entities;

public enum PaymentMethod { Cash, BankTransfer }

public class PaymentReceipt
{
    public int Id { get; set; }
    public string ReceiptNumber { get; set; } = "";        // PT-YYYYMMDD-NNN
    public int CustomerId { get; set; }
    public Customer Customer { get; set; } = null!;
    public int? EmployeeId { get; set; }                   // NV bán hàng (nullable)
    public Employee? Employee { get; set; }
    public DateTime CollectionDate { get; set; }
    public string? Description { get; set; }
    public decimal TotalAmount { get; set; }
    public PaymentMethod PaymentMethod { get; set; } = PaymentMethod.Cash;
    public string Currency { get; set; } = "VND";
    public decimal ExchangeRate { get; set; } = 1m;
    public DateTime CreatedAt { get; set; }
    public ICollection<PaymentReceiptLine> Lines { get; set; } = new List<PaymentReceiptLine>();
}
