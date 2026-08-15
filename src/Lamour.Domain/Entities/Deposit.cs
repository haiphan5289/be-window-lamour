namespace Lamour.Domain.Entities;

public enum DepositStatus
{
    Active   = 0,  // Còn số dư
    Depleted = 1,  // Đã trừ hết (RemainingBalance == 0)
}

public class Deposit
{
    public int      Id             { get; set; }
    public string   DocumentNumber { get; set; } = "";   // DC prefix

    public DateTime AccountingDate { get; set; }
    public DateTime DocumentDate   { get; set; }

    public int       CustomerId { get; set; }
    public Customer  Customer   { get; set; } = null!;

    public int?      EmployeeId { get; set; }             // NV nhận cọc
    public Employee? Employee   { get; set; }

    public string?  Description { get; set; }             // Diễn giải
    public string?  Reference   { get; set; }              // Tham chiếu

    public decimal        Amount           { get; set; }   // Số tiền cọc ban đầu
    public decimal        RemainingBalance { get; set; }    // Số dư còn lại
    public DepositStatus  Status           { get; set; } = DepositStatus.Active;
    public DateTime       CreatedAt        { get; set; }

    // Đơn bán hàng (Chứng từ bán hàng) đã tạo ra cọc này qua 1 dòng sản phẩm "Đặt cọc"
    // (Product.IsDepositProduct = true) — null nếu cọc được tạo thủ công qua màn Đặt Cọc.
    public int?        SourceSalesOrderId { get; set; }
    public SalesOrder? SourceSalesOrder   { get; set; }

    public ICollection<DepositDeduction> Deductions { get; set; } = new List<DepositDeduction>();
}
