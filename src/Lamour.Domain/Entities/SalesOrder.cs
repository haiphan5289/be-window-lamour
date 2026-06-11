namespace Lamour.Domain.Entities;

public enum SalesOrderStatus
{
    Normal    = 0,
    Held      = 1,  // Treo đơn — chờ KH xác nhận
    Confirmed = 2,  // Đã xác nhận — bất biến
}

public class SalesOrder
{
    public int      Id             { get; set; }
    public string   DocumentNumber { get; set; } = "";   // BH prefix

    public DateTime AccountingDate { get; set; }
    public DateTime DocumentDate   { get; set; }

    public int       CustomerId { get; set; }
    public Customer  Customer   { get; set; } = null!;

    public int?      EmployeeId { get; set; }             // NV bán hàng
    public Employee? Employee   { get; set; }

    public string?  Description    { get; set; }          // Diễn giải
    public string?  Reference      { get; set; }          // Tham chiếu

    // Điều khoản thanh toán
    public string?   PaymentTerms   { get; set; }         // Điều khoản TT
    public int?      PaymentDueDays { get; set; }         // Số ngày được nợ
    public DateTime? PaymentDueDate { get; set; }         // Hạn thanh toán

    // Thông tin bổ sung (Tab 6)
    public string?  Notes          { get; set; }          // Ghi Chú
    public string?  DeliveryMethod { get; set; }          // PT Giao hàng
    public string?  PaymentMethod  { get; set; }          // PT thanh toán

    public decimal           TotalAmount { get; set; }
    public DateTime          CreatedAt   { get; set; }
    public SalesOrderStatus  Status      { get; set; } = SalesOrderStatus.Normal;

    public ICollection<SalesOrderLine> Lines { get; set; } = new List<SalesOrderLine>();
}

public class SalesOrderLine
{
    public int        Id           { get; set; }
    public int        SalesOrderId { get; set; }
    public SalesOrder SalesOrder   { get; set; } = null!;

    public int     ProductId   { get; set; }
    public Product Product     { get; set; } = null!;

    public string  ProductCode { get; set; } = "";        // Mã hàng (denormalized)
    public string  ProductName { get; set; } = "";        // Tên hàng (denormalized)
    public bool    IsPromotion { get; set; }              // Hàng khuyến mại

    public string  Unit         { get; set; } = "";        // ĐVT
    public int     Quantity     { get; set; }              // Số lượng
    public decimal UnitPrice    { get; set; }              // Đơn giá
    public decimal DiscountRate { get; set; }              // Tỷ lệ CK (%)
    public decimal Amount       { get; set; }              // Thành tiền (net)

    public string ReceivableAccount { get; set; } = "131"; // TK công nợ/chi phí
    public string RevenueAccount    { get; set; } = "511"; // TK doanh thu
}
