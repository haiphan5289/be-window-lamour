using Lamour.Domain.Enums;

namespace Lamour.Domain.Entities;

public enum ReceiptStatus
{
    Draft     = 0,
    Confirmed = 1,
}

public class Receipt
{
    public int Id { get; set; }
    // Null cho "Phiếu thu tiền khách hàng hàng loạt" (1 phiếu, nhiều khách hàng khác nhau — mỗi
    // dòng hạch toán tự mang khách hàng riêng qua ReceiptEntry.SubjectCode/SubjectName). Non-null
    // cho phiếu thu 1 khách hàng bình thường (hành vi cũ, không đổi).
    public int?      CustomerId { get; set; }
    public Customer? Customer   { get; set; }
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
    public ReceiptStatus Status      { get; set; } = ReceiptStatus.Draft;
    public DateTime?     ConfirmedAt { get; set; }
    public DateTime CreatedAt { get; set; }
    public ICollection<ReceiptEntry> Entries { get; set; } = new List<ReceiptEntry>();
}
