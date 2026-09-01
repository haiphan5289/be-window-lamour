namespace Lamour.Domain.Entities;

public enum SalesReturnType
{
    ReduceDebt  = 0,  // Giảm trừ công nợ
    CashRefund  = 1,  // Trả lại tiền mặt
}

public enum SalesReturnStatus
{
    Draft     = 0,
    Confirmed = 1,
}

public class SalesReturn
{
    public int    Id             { get; set; }
    public string DocumentNumber { get; set; } = "";  // BTL prefix

    public SalesReturnStatus Status      { get; set; } = SalesReturnStatus.Draft;
    public DateTime?         ConfirmedAt { get; set; }

    public DateTime AccountingDate { get; set; }
    public DateTime DocumentDate   { get; set; }

    public int      CustomerId { get; set; }
    public Customer Customer   { get; set; } = null!;

    public int?      EmployeeId { get; set; }
    public Employee? Employee   { get; set; }

    public string? Description { get; set; }
    public string? Reference   { get; set; }

    public SalesReturnType ReturnType { get; set; } = SalesReturnType.ReduceDebt;

    public decimal TotalAmount   { get; set; }  // Tổng tiền hàng (gross = sum qty×price)
    public decimal TotalDiscount { get; set; }  // Tổng chiết khấu
    public decimal TotalPayment  { get; set; }  // = TotalAmount - TotalDiscount

    public DateTime CreatedAt { get; set; }

    public ICollection<SalesReturnLine> Lines { get; set; } = new List<SalesReturnLine>();
}

public class SalesReturnLine
{
    public int         Id            { get; set; }
    public int         SalesReturnId { get; set; }
    public SalesReturn SalesReturn   { get; set; } = null!;

    public int     ProductId   { get; set; }
    public Product Product     { get; set; } = null!;

    public int       WarehouseId { get; set; }      // Kho nhận lại hàng cho dòng này
    public Warehouse Warehouse   { get; set; } = null!;

    public string ProductCode { get; set; } = "";  // denormalized
    public string ProductName { get; set; } = "";  // denormalized

    public string ReturnAccount   { get; set; } = "5212";  // TK trả lại
    public string DebtAccount     { get; set; } = "131";   // TK công nợ
    public string DiscountAccount { get; set; } = "5211";  // TK chiết khấu

    public string  Unit      { get; set; } = "";
    public int     Quantity  { get; set; }
    public decimal UnitPrice { get; set; }

    public decimal Amount         { get; set; }  // gross = qty × unit_price
    public decimal DiscountRate   { get; set; }  // % (0–100)
    public decimal DiscountAmount { get; set; }  // = Amount × DiscountRate / 100

    public string? SalesOrderNumber { get; set; }  // Số CT bán hàng (reference per line)

    // Tab "2. Thuế" — denormalized từ Product.VatRate tại thời điểm ghi sổ, giống SalesOrderLine.
    public decimal TaxRate    { get; set; }             // % thuế GTGT
    public decimal TaxAmount  { get; set; }              // = (Amount - DiscountAmount) × TaxRate / 100
    public string  TaxAccount { get; set; } = "33311";   // TK thuế GTGT

    // Tab "3. Giá vốn" — Kho dùng chung WarehouseId ở trên; CostPrice denormalized từ Product.CostPrice.
    public string  CostAccount { get; set; } = "1561";   // TK kho (giá vốn)
    public string  CogsAccount { get; set; } = "632";    // TK giá vốn
    public decimal CostPrice   { get; set; }              // Đơn giá vốn
    public decimal CostAmount  { get; set; }              // = Quantity × CostPrice

    // Tab "4. Thống kê" — chỉ "Đơn vị" (Department), 6 field còn lại của MISA (Công trình,
    // Đơn đặt hàng, Hợp đồng bán, Đối tượng THCP, Số khế ước, Mã thống kê) không có master data
    // trong hệ thống này, bỏ qua theo yêu cầu.
    public int?         DepartmentId { get; set; }
    public Department?  Department   { get; set; }
}
