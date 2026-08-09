namespace Lamour.Domain.Entities;

public class DepositDeduction
{
    public int      Id             { get; set; }
    public string   DocumentNumber { get; set; } = "";   // TC prefix

    public int      DepositId { get; set; }
    public Deposit  Deposit   { get; set; } = null!;

    public int         SalesOrderId { get; set; }         // Luôn gắn với 1 Chứng từ bán hàng
    public SalesOrder  SalesOrder   { get; set; } = null!;

    public decimal  Amount { get; set; }                   // Số tiền trừ lần này

    public DateTime AccountingDate { get; set; }
    public DateTime DocumentDate   { get; set; }
    public string?  Description    { get; set; }

    public DateTime CreatedAt { get; set; }
}
