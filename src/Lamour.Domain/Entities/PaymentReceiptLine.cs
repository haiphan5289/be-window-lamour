namespace Lamour.Domain.Entities;

public class PaymentReceiptLine
{
    public int Id { get; set; }
    public int PaymentReceiptId { get; set; }
    public PaymentReceipt PaymentReceipt { get; set; } = null!;
    public DateTime DocumentDate { get; set; }      // Ngày chứng từ
    public string DocumentNumber { get; set; } = ""; // Số chứng từ
    public string InvoiceNumber { get; set; } = "";  // Số hóa đơn (string ref, no FK yet)
    public string Description { get; set; } = "";    // Diễn giải
    public DateTime? DueDate { get; set; }           // Hạn thanh toán
    public decimal AmountDue { get; set; }           // Số phải thu
    public decimal AmountPaid { get; set; }          // Số thanh toán
}
