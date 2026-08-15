using Lamour.Domain.Enums;


namespace Lamour.Domain.Entities;

public class Product
{
    public int          Id                { get; set; }
    public string       Code              { get; set; } = string.Empty;
    public string       Name              { get; set; } = string.Empty;
    public int?         CategoryId        { get; set; }
    public Category?    Category          { get; set; }
    public string       Unit              { get; set; } = string.Empty;
    public decimal      CostPrice         { get; set; }
    public decimal      SellingPrice      { get; set; }
    public int          StockQuantity     { get; set; }
    public bool         IsActive          { get; set; } = true;

    // Tax fields
    public VatRateType? VatRate           { get; set; }
    public TaxReductionStatus? TaxReductionType  { get; set; }
    public decimal?     ImportTaxRate     { get; set; }
    public decimal?     ExportTaxRate     { get; set; }
    public string?      ExciseTaxGroup    { get; set; }

    // Header fields — "Sửa Vật tư, hàng hoá, dịch vụ" popup (2026-08-09)
    public ProductNature Nature              { get; set; } = ProductNature.VatTuHangHoa;
    public string?        Description         { get; set; }
    // ĐVT chính — FK bổ sung; Unit (string) phía trên vẫn giữ nguyên để không phá vỡ
    // Sales/SalesReturn/WarehouseReceipt đang đọc trực tiếp product.Unit làm giá trị mặc định.
    public int?            ProductUnitId       { get; set; }
    public ProductUnit?    ProductUnit         { get; set; }
    public string?         WarrantyPeriod      { get; set; }
    public int             MinStockQuantity    { get; set; }
    public string?         Origin              { get; set; }
    public string?         PurchaseDescription { get; set; }
    public string?         SaleDescription     { get; set; }

    // Tab "Ngầm định"
    public int?            DefaultWarehouseId      { get; set; }
    public Warehouse?      DefaultWarehouse        { get; set; }
    public int?            StockAccountId          { get; set; }
    public AccountSetting? StockAccount            { get; set; }
    public int?            RevenueAccountId        { get; set; }
    public AccountSetting? RevenueAccount          { get; set; }
    public int?            DiscountAccountId       { get; set; }
    public AccountSetting? DiscountAccount         { get; set; }
    public int?            PriceReductionAccountId { get; set; }
    public AccountSetting? PriceReductionAccount   { get; set; }
    public int?            ReturnAccountId         { get; set; }
    public AccountSetting? ReturnAccount           { get; set; }
    public int?            CostAccountId           { get; set; }
    public AccountSetting? CostAccount             { get; set; }
    public decimal         TradeDiscountRate       { get; set; }
    public string?         SpecialGoodsType        { get; set; }
    public decimal         LatestPurchasePrice     { get; set; }
    public bool            IsPromotionalGood       { get; set; }
    // Sản phẩm đại diện cho việc "Đặt cọc" — dòng dùng sản phẩm này trong Sales Order sẽ tự động
    // tạo/đồng bộ 1 Deposit ngầm gắn với đơn hàng đó (xem SalesOrderDepositHelper).
    public bool            IsDepositProduct        { get; set; }
}
