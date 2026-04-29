using Lamour.Domain.Enums;

namespace Lamour.Domain.Entities;

public class ReceiptEntry
{
    public int Id { get; set; }
    public int ReceiptId { get; set; }
    public Receipt Receipt { get; set; } = null!;
    public string Description { get; set; } = "";    // Diễn giải
    public AccountCode DebitAccount { get; set; }    // TK Nợ
    public AccountCode CreditAccount { get; set; }   // TK Có
    public decimal Amount { get; set; }               // Số tiền
    public string? SubjectCode { get; set; }          // Đối tượng column
    public string? SubjectName { get; set; }          // Tên đối tượng
    public string? BankAccount { get; set; }          // TK ngân hàng
}
