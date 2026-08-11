using Lamour.Application.Abstractions;
using Lamour.Application.Features.Categories.Repositories;
using Lamour.Application.Features.Products.Dtos;
using Lamour.Application.Features.Products.Repositories;
using Lamour.Application.Features.ProductUnits.Repositories;
using Lamour.Application.Features.Warehouse.Repositories;
using Lamour.Domain.Entities;
using Lamour.Domain.Enums;
using Lamour.Domain.Exceptions;
using Microsoft.Extensions.Logging;

namespace Lamour.Application.Features.Products.UseCases;

public class CreateProductUseCase : ICreateProductUseCase
{
    private readonly IProductRepository          _repo;
    private readonly ICategoryRepository         _categoryRepo;
    private readonly IProductUnitRepository      _productUnitRepo;
    private readonly IProductWarehouseStockRepository _stockRepo;
    private readonly INotificationBroadcaster    _broadcaster;
    private readonly ILogger<CreateProductUseCase> _logger;

    public CreateProductUseCase(
        IProductRepository repo,
        ICategoryRepository categoryRepo,
        IProductUnitRepository productUnitRepo,
        IProductWarehouseStockRepository stockRepo,
        INotificationBroadcaster broadcaster,
        ILogger<CreateProductUseCase> logger)
    {
        _repo            = repo;
        _categoryRepo    = categoryRepo;
        _productUnitRepo = productUnitRepo;
        _stockRepo       = stockRepo;
        _broadcaster     = broadcaster;
        _logger          = logger;
    }

    public async Task<ProductResponseDto> ExecuteAsync(CreateProductRequestDto request, CancellationToken ct = default)
    {
        if (string.IsNullOrWhiteSpace(request.Name))
            throw new DomainException("Product name is required.");

        var category = await _categoryRepo.GetByIdAsync(request.CategoryId, ct)
            ?? throw new DomainException($"Danh mục với id {request.CategoryId} không tồn tại.");

        if (!string.IsNullOrWhiteSpace(request.Code) && await _repo.CodeExistsAsync(request.Code, ct: ct))
            throw new DomainException($"Product with code '{request.Code}' already exists.");

        ProductUnit? productUnit = null;
        if (request.ProductUnitId.HasValue)
        {
            productUnit = await _productUnitRepo.GetByIdAsync(request.ProductUnitId.Value, ct)
                ?? throw new DomainException($"Đơn vị tính với id {request.ProductUnitId} không tồn tại.");
        }

        var product = new Product
        {
            Code             = request.Code.Trim(),
            Name             = request.Name.Trim(),
            CategoryId       = request.CategoryId,
            // ĐVT chính (nếu chọn) đồng bộ vào Unit (string) để Sales/SalesReturn/WarehouseReceipt
            // dùng làm mặc định như trước — không phá vỡ luồng hiện có.
            Unit             = productUnit?.Name ?? request.Unit,
            CostPrice        = request.CostPrice,
            SellingPrice     = request.SellingPrice,
            StockQuantity    = request.StockQuantity,
            IsActive         = request.IsActive,
            VatRate          = ParseVatRate(request.VatRate),
            TaxReductionType = ParseTaxReductionStatus(request.TaxReductionType),
            ImportTaxRate    = request.ImportTaxRate,
            ExportTaxRate    = request.ExportTaxRate,
            ExciseTaxGroup   = request.ExciseTaxGroup,

            Nature              = ParseProductNature(request.Nature),
            Description         = request.Description,
            ProductUnitId       = request.ProductUnitId,
            WarrantyPeriod      = request.WarrantyPeriod,
            MinStockQuantity    = request.MinStockQuantity,
            Origin              = request.Origin,
            PurchaseDescription = request.PurchaseDescription,
            SaleDescription     = request.SaleDescription,

            DefaultWarehouseId      = request.DefaultWarehouseId,
            StockAccountId          = request.StockAccountId,
            RevenueAccountId        = request.RevenueAccountId,
            DiscountAccountId       = request.DiscountAccountId,
            PriceReductionAccountId = request.PriceReductionAccountId,
            ReturnAccountId         = request.ReturnAccountId,
            CostAccountId           = request.CostAccountId,
            TradeDiscountRate       = request.TradeDiscountRate,
            SpecialGoodsType        = request.SpecialGoodsType,
            LatestPurchasePrice     = request.LatestPurchasePrice,
            IsPromotionalGood       = request.IsPromotionalGood,
        };

        var created = await _repo.AddAsync(product, ct);
        created.Category = category;
        _logger.LogInformation("Created product {Id} '{Name}'", created.Id, created.Name);

        // Ghi nhận tồn kho ban đầu vào đúng kho ngầm định (nếu có chọn) — chỉ khi tạo mới,
        // không cho phép set lại qua Update sau này (xem UpdateProductUseCase).
        if (created.StockQuantity != 0 && request.DefaultWarehouseId.HasValue)
            await _stockRepo.AdjustQuantityAsync(created.Id, request.DefaultWarehouseId.Value, created.StockQuantity, ct);

        // Re-fetch với đầy đủ Include (ProductUnit/Warehouse/AccountSettings) để map DTO có tên hiển thị.
        var reloaded = await _repo.GetByIdAsync(created.Id, ct) ?? created;
        var dto = MapToDto(reloaded);
        await _broadcaster.ProductCreatedAsync(dto, ct);
        return dto;
    }

    internal static VatRateType? ParseVatRate(string? value) =>
        Enum.TryParse<VatRateType>(value, out var result) ? result : null;

    internal static TaxReductionStatus? ParseTaxReductionStatus(string? value) =>
        Enum.TryParse<TaxReductionStatus>(value, out var result) ? result : null;

    internal static ProductNature ParseProductNature(string? value) =>
        Enum.TryParse<ProductNature>(value, out var result) ? result : ProductNature.VatTuHangHoa;

    internal static ProductResponseDto MapToDto(Product p) => new()
    {
        Id               = p.Id,
        Code             = p.Code,
        Name             = p.Name,
        CategoryId       = p.CategoryId,
        CategoryName     = p.Category?.Name ?? string.Empty,
        Unit             = p.Unit,
        CostPrice        = p.CostPrice,
        SellingPrice     = p.SellingPrice,
        StockQuantity    = p.StockQuantity,
        IsActive         = p.IsActive,
        VatRate          = p.VatRate?.ToString(),
        TaxReductionType = p.TaxReductionType?.ToString(),
        ImportTaxRate    = p.ImportTaxRate,
        ExportTaxRate    = p.ExportTaxRate,
        ExciseTaxGroup   = p.ExciseTaxGroup,

        Nature              = p.Nature.ToString(),
        Description         = p.Description,
        ProductUnitId       = p.ProductUnitId,
        ProductUnitName     = p.ProductUnit?.Name,
        WarrantyPeriod      = p.WarrantyPeriod,
        MinStockQuantity    = p.MinStockQuantity,
        Origin              = p.Origin,
        PurchaseDescription = p.PurchaseDescription,
        SaleDescription     = p.SaleDescription,

        DefaultWarehouseId        = p.DefaultWarehouseId,
        DefaultWarehouseName      = p.DefaultWarehouse?.Name,
        StockAccountId            = p.StockAccountId,
        StockAccountCode          = p.StockAccount?.Code,
        RevenueAccountId          = p.RevenueAccountId,
        RevenueAccountCode        = p.RevenueAccount?.Code,
        DiscountAccountId         = p.DiscountAccountId,
        DiscountAccountCode       = p.DiscountAccount?.Code,
        PriceReductionAccountId   = p.PriceReductionAccountId,
        PriceReductionAccountCode = p.PriceReductionAccount?.Code,
        ReturnAccountId           = p.ReturnAccountId,
        ReturnAccountCode         = p.ReturnAccount?.Code,
        CostAccountId             = p.CostAccountId,
        CostAccountCode           = p.CostAccount?.Code,
        TradeDiscountRate         = p.TradeDiscountRate,
        SpecialGoodsType          = p.SpecialGoodsType,
        LatestPurchasePrice       = p.LatestPurchasePrice,
        IsPromotionalGood         = p.IsPromotionalGood,
    };
}
