using Lamour.Domain.Enums;

namespace Lamour.Domain.Entities;

public enum PaymentStatus
{
    Draft     = 0,
    Treo      = 1,
    Confirmed = 2,
}

/// <summary>
/// Payment Voucher (Phiếu Chi) - money out transaction
/// </summary>
public class Payment
{
    public int Id { get; set; }
    public int SupplierId { get; set; }
    public Supplier Supplier { get; set; } = null!;
    public string PayeeName { get; set; } = "";                // Người nhận
    public string? Address { get; set; }                        // Địa chỉ
    public PaymentReason PaymentReason { get; set; } = PaymentReason.ChiKhac;
    public string? ReasonDetail { get; set; }                    // Lý do chi (chi tiết, tự do)
    public int? PaymentEmployeeId { get; set; }                 // Nhân viên chi
    public Employee? PaymentEmployee { get; set; }
    public string? Attachment { get; set; }                     // Kèm theo
    public string? Reference { get; set; }                      // Tham chiếu
    public DateTime AccountingDate { get; set; }                // Ngày hạch toán
    public DateTime DocumentDate { get; set; }                  // Ngày chứng từ
    public string DocumentNumber { get; set; } = "";            // Số chứng từ — user input
    public PaymentStatus Status { get; set; } = PaymentStatus.Draft;
    public DateTime CreatedAt { get; set; }
    public DateTime? ConfirmedAt { get; set; }
    public ICollection<PaymentEntry> Entries { get; set; } = new List<PaymentEntry>();
}
