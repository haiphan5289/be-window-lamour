namespace Lamour.Domain.Entities;

/// <summary>
/// Payment Entry (line item for Phiếu Chi)
/// </summary>
public class PaymentEntry
{
    public int Id { get; set; }
    public int PaymentId { get; set; }
    public Payment Payment { get; set; } = null!;
    public string Description { get; set; } = "";    // Diễn giải
    public int DebitAccountSettingId { get; set; }    // TK Nợ
    public AccountSetting DebitAccountSetting { get; set; } = null!;
    public int CreditAccountSettingId { get; set; }   // TK Có
    public AccountSetting CreditAccountSetting { get; set; } = null!;
    public decimal Amount { get; set; }               // Số tiền
    public string? SubjectCode { get; set; }          // Đối tượng column
    public string? SubjectName { get; set; }          // Tên đối tượng
    public string? BankAccount { get; set; }          // TK ngân hàng
    public int? ExpenseCategoryId { get; set; }        // Khoản mục CP
    public ExpenseCategory? ExpenseCategory { get; set; }
}
