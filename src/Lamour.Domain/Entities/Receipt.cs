using Lamour.Domain.Enums;

namespace Lamour.Domain.Entities;

public class Receipt
{
    public int Id { get; set; }
    public int CustomerId { get; set; }
    public Customer Customer { get; set; } = null!;
    public string PayerName { get; set; } = "";           // Người nộp
    public string? Address { get; set; }                   // Địa chỉ
    public PaymentReason PaymentReason { get; set; } = PaymentReason.ThuKhac;
    public int? CollectorEmployeeId { get; set; }          // Nhân viên thu
    public Employee? CollectorEmployee { get; set; }
    public string? Attachment { get; set; }                // Kèm theo
    public string? Reference { get; set; }                 // Tham chiếu
    public DateTime AccountingDate { get; set; }           // Ngày hạch toán
    public DateTime DocumentDate { get; set; }             // Ngày chứng từ
    public string DocumentNumber { get; set; } = "";       // Số chứng từ — user input
    public DateTime CreatedAt { get; set; }
    public ICollection<ReceiptEntry> Entries { get; set; } = new List<ReceiptEntry>();
}
