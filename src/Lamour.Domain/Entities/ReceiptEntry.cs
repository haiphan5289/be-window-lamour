namespace Lamour.Domain.Entities;

public class ReceiptEntry
{
    public int Id { get; set; }
    public int ReceiptId { get; set; }
    public Receipt Receipt { get; set; } = null!;
    public string Description { get; set; } = "";    // Diễn giải
    public int DebitAccountSettingId { get; set; }    // TK Nợ
    public AccountSetting DebitAccountSetting { get; set; } = null!;
    public int CreditAccountSettingId { get; set; }   // TK Có
    public AccountSetting CreditAccountSetting { get; set; } = null!;
    public decimal Amount { get; set; }               // Số tiền
    public string? SubjectCode { get; set; }          // Đối tượng column
    public string? SubjectName { get; set; }          // Tên đối tượng
    public string? BankAccount { get; set; }          // TK ngân hàng

    // Chứng từ bán hàng gốc đang được thu tiền (Phiếu thu hàng loạt khách hàng) — null cho phiếu
    // thu bình thường không gắn với 1 đơn hàng cụ thể (ThuKhac...).
    public int?        SalesOrderId { get; set; }
    public SalesOrder? SalesOrder   { get; set; }
}
