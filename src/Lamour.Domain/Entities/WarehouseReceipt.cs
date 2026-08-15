namespace Lamour.Domain.Entities;

public enum WarehouseReceiptType
{
    FinishedGoodsProduced = 1,  // Thành phẩm sản xuất
    ReturnedGoods         = 2,  // Hàng bán bị trả lại
    Other                 = 3,  // Khác (NVL thừa, HH thuê gia công,...)
    ProcessingReceived    = 4,  // Hàng nhận gia công
}

public enum WarehouseReceiptStatus
{
    Draft     = 0,
    Confirmed = 1,
}

public class WarehouseReceipt
{
    public int    Id            { get; set; }
    public string ReceiptNumber { get; set; } = "";  // NK00048

    public WarehouseReceiptType   ReceiptType { get; set; }
    public WarehouseReceiptStatus Status      { get; set; } = WarehouseReceiptStatus.Draft;

    public int?      CustomerId { get; set; }
    public Customer? Customer   { get; set; }

    public int?      SupplierId { get; set; }
    public Supplier?  Supplier  { get; set; }

    public int?      EmployeeId { get; set; }
    public Employee? Employee   { get; set; }

    public DateTime  AccountingDate { get; set; }
    public DateTime  DocumentDate   { get; set; }
    public string?   Description    { get; set; }
    public string?   DeliveryPerson { get; set; }
    public string?   Reference      { get; set; }

    public decimal   TotalAmount  { get; set; }
    public DateTime  CreatedAt    { get; set; }
    public DateTime? ConfirmedAt  { get; set; }

    public ICollection<WarehouseReceiptLine> Lines { get; set; } = new List<WarehouseReceiptLine>();
}

public class WarehouseReceiptLine
{
    public int              Id                 { get; set; }
    public int              WarehouseReceiptId { get; set; }
    public WarehouseReceipt WarehouseReceipt   { get; set; } = null!;

    public int     ProductId { get; set; }
    public Product Product   { get; set; } = null!;

    public int       WarehouseId { get; set; }
    public Warehouse Warehouse   { get; set; } = null!;

    public int     Quantity      { get; set; }
    public decimal UnitPrice     { get; set; }
    public decimal Amount        { get; set; }

    public string DebitAccount  { get; set; } = "111";
    public string CreditAccount { get; set; } = "131";

    // Thống kê — cột kế toán mở rộng (theo dõi nội bộ, không có bảng danh mục riêng)
    public string? CostItem            { get; set; }  // Khoản mục CP
    public string? CostObject          { get; set; }  // Đối tượng THCP
    public string? Project             { get; set; }  // Công trình
    public string? PurchaseOrderNumber { get; set; }  // Đơn đặt hàng
    public string? SalesContractNumber { get; set; }  // Hợp đồng bán
    public string? LoanContractNumber  { get; set; }  // Số khế ước
    public string? StatisticsCode      { get; set; }  // Mã thống kê
}
