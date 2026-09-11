namespace Lamour.Domain.Entities;

public enum SalesOrderStatus
{
    Normal  = 0,  // Ghi sổ — mặc định khi tạo đơn
    Held    = 1,  // Treo đơn — 2026-09-11: giờ là trạng thái "chưa Ghi sổ" DUY NHẤT (gộp với Draft,
                  // xem SalesReturnStatus.Held cho bối cảnh đầy đủ). UnconfirmSalesOrderUseCase
                  // ("Bỏ ghi") giờ luôn đưa đơn về Held thay vì Draft.
    // Draft — GIỮ LẠI giá trị enum để tương thích ngược (dữ liệu cũ/serialize), nhưng từ 2026-09-11
    // KHÔNG còn code nào gán mới giá trị này nữa — mọi nơi đọc Status coi Draft và Held là như nhau
    // (xem SalesOrderViewModel.IsHeld, SalesOrderListItem.StatusLabel).
    Draft   = 2,
}

public class SalesOrder
{
    public int      Id             { get; set; }
    public string   DocumentNumber { get; set; } = "";   // BH prefix

    public DateTime AccountingDate { get; set; }
    public DateTime DocumentDate   { get; set; }

    public int       CustomerId { get; set; }
    public Customer  Customer   { get; set; } = null!;
    // Tên khách hàng hiển thị tuỳ chỉnh cho riêng chứng từ này (khác Customer.Name thật) — null
    // nghĩa là chưa override, dùng thẳng Customer.Name. Không đổi CustomerId/công nợ liên quan.
    public string?   CustomerNameOverride { get; set; }
    // Địa chỉ hiển thị tuỳ chỉnh cho riêng chứng từ này (khác Customer.Address thật) — cùng cơ chế
    // với CustomerNameOverride, null nghĩa là chưa override, dùng thẳng Customer.Address.
    public string?   CustomerAddressOverride { get; set; }

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

    public decimal           TotalAmount    { get; set; }          // Tiền hàng (net, chưa thuế)
    public decimal           TotalTaxAmount { get; set; }          // Tổng tiền thuế
    public decimal           GrandTotal     { get; set; }          // TotalAmount + TotalTaxAmount
    public DateTime          CreatedAt      { get; set; }
    public SalesOrderStatus  Status         { get; set; } = SalesOrderStatus.Normal;

    public ICollection<SalesOrderLine> Lines { get; set; } = new List<SalesOrderLine>();
}

public class SalesOrderLine
{
    public int        Id           { get; set; }
    public int        SalesOrderId { get; set; }
    public SalesOrder SalesOrder   { get; set; } = null!;

    public int     ProductId   { get; set; }
    public Product Product     { get; set; } = null!;

    public int?       WarehouseId { get; set; }            // Kho xuất hàng cho dòng này — null nếu là dòng "Đặt cọc" (không phải hàng tồn kho thật)
    public Warehouse? Warehouse   { get; set; }

    public string  ProductCode { get; set; } = "";        // Mã hàng (denormalized)
    public string  ProductName { get; set; } = "";        // Tên hàng (denormalized)
    public bool    IsPromotion { get; set; }              // Hàng khuyến mại
    public bool    IsDepositProduct { get; set; }         // Denormalized từ Product.IsDepositProduct tại thời điểm ghi sổ — dùng để ẩn Đơn giá/CK/Thuế suất trên hóa đơn in

    public string  Unit         { get; set; } = "";        // ĐVT
    public int     Quantity     { get; set; }              // Số lượng
    public decimal UnitPrice    { get; set; }              // Đơn giá
    public decimal DiscountRate { get; set; }              // Tỷ lệ CK (%)
    public decimal Amount       { get; set; }              // Thành tiền (net, chưa thuế)
    public bool    IsAmountManual { get; set; }             // true = Amount do user gõ tay, bỏ qua công thức Qty×UnitPrice×(1-CK%)

    public decimal TaxRate      { get; set; }              // Thuế suất (%) — denormalized từ Product.VatRate tại thời điểm ghi sổ
    public decimal TaxAmount    { get; set; }              // Tiền thuế = Amount * TaxRate / 100

    public string ReceivableAccount { get; set; } = "131"; // TK công nợ/chi phí
    public string RevenueAccount    { get; set; } = "511"; // TK doanh thu
}
