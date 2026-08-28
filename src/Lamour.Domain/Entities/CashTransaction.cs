namespace Lamour.Domain.Entities;

public class CashTransaction
{
    public int Id { get; set; }
    public DateTime AccountingDate { get; set; }   // Ngày hạch toán
    public DateTime DocumentDate { get; set; }      // Ngày chứng từ
    public string? ReceiptNumber { get; set; }      // Số phiếu thu (PT00678) — null if payment
    public string? PaymentNumber { get; set; }      // Số phiếu chi (PC02215) — null if receipt
    public string Description { get; set; } = "";   // Diễn giải
    public string Account { get; set; } = "111";    // Tài khoản (always 111 for cash)
    public string CounterAccount { get; set; } = ""; // TK đối ứng
    public decimal DebitAmount { get; set; }         // Nợ (tiền vào - PT)
    public decimal CreditAmount { get; set; }        // Có (tiền ra - PC)
    public string? PersonName { get; set; }          // Người nhận/Người nộp
    public string? PaymentReason { get; set; }       // Lý do thu/chi — Domain.Enums.PaymentReason as string
    public string DocumentType { get; set; } = "";   // Loại chứng từ (VD: "Phiếu chi", "Phiếu thu tiền mặt khách hàng")
    public DateTime CreatedAt { get; set; }
}
