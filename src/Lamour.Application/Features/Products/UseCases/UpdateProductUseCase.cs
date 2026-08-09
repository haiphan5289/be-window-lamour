using Lamour.Application.Abstractions;
using Lamour.Application.Features.Categories.Repositories;
using Lamour.Application.Features.Products.Dtos;
using Lamour.Application.Features.Products.Repositories;
using Lamour.Application.Features.ProductUnits.Repositories;
using Lamour.Domain.Entities;
using Lamour.Domain.Exceptions;
using Microsoft.Extensions.Logging;

namespace Lamour.Application.Features.Products.UseCases;

public class UpdateProductUseCase : IUpdateProductUseCase
{
    private readonly IProductRepository          _repo;
    private readonly ICategoryRepository         _categoryRepo;
    private readonly IProductUnitRepository      _productUnitRepo;
    private readonly INotificationBroadcaster    _broadcaster;
    private readonly ILogger<UpdateProductUseCase> _logger;

    public UpdateProductUseCase(
        IProductRepository repo,
        ICategoryRepository categoryRepo,
        IProductUnitRepository productUnitRepo,
        INotificationBroadcaster broadcaster,
        ILogger<UpdateProductUseCase> logger)
    {
        _repo            = repo;
        _categoryRepo    = categoryRepo;
        _productUnitRepo = productUnitRepo;
        _broadcaster     = broadcaster;
        _logger          = logger;
    }

    public async Task<ProductResponseDto> ExecuteAsync(int id, UpdateProductRequestDto request, CancellationToken ct = default)
    {
        var product = await _repo.GetByIdAsync(id, ct)
            ?? throw new NotFoundException($"Product {id} not found.");

        if (string.IsNullOrWhiteSpace(request.Name))
            throw new DomainException("Product name is required.");

        var category = await _categoryRepo.GetByIdAsync(request.CategoryId, ct)
            ?? throw new DomainException($"Danh mục với id {request.CategoryId} không tồn tại.");

        if (!string.IsNullOrWhiteSpace(request.Code) && await _repo.CodeExistsAsync(request.Code, excludeId: id, ct: ct))
            throw new DomainException($"Product with code '{request.Code}' already exists.");

        ProductUnit? productUnit = null;
        if (request.ProductUnitId.HasValue)
        {
            productUnit = await _productUnitRepo.GetByIdAsync(request.ProductUnitId.Value, ct)
                ?? throw new DomainException($"Đơn vị tính với id {request.ProductUnitId} không tồn tại.");
        }

        product.Code             = request.Code.Trim();
        product.Name             = request.Name.Trim();
        product.CategoryId       = request.CategoryId;
        product.Unit             = productUnit?.Name ?? request.Unit;
        product.CostPrice        = request.CostPrice;
        product.SellingPrice     = request.SellingPrice;
        product.StockQuantity    = request.StockQuantity;
        product.IsActive         = request.IsActive;
        product.VatRate          = CreateProductUseCase.ParseVatRate(request.VatRate);
        product.TaxReductionType = CreateProductUseCase.ParseTaxReductionStatus(request.TaxReductionType);
        product.ImportTaxRate    = request.ImportTaxRate;
        product.ExportTaxRate    = request.ExportTaxRate;
        product.ExciseTaxGroup   = request.ExciseTaxGroup;

        product.Nature              = CreateProductUseCase.ParseProductNature(request.Nature);
        product.Description         = request.Description;
        product.ProductUnitId       = request.ProductUnitId;
        product.WarrantyPeriod      = request.WarrantyPeriod;
        product.MinStockQuantity    = request.MinStockQuantity;
        product.Origin              = request.Origin;
        product.PurchaseDescription = request.PurchaseDescription;
        product.SaleDescription     = request.SaleDescription;

        product.DefaultWarehouseId      = request.DefaultWarehouseId;
        product.StockAccountId          = request.StockAccountId;
        product.RevenueAccountId        = request.RevenueAccountId;
        product.DiscountAccountId       = request.DiscountAccountId;
        product.PriceReductionAccountId = request.PriceReductionAccountId;
        product.ReturnAccountId         = request.ReturnAccountId;
        product.CostAccountId           = request.CostAccountId;
        product.TradeDiscountRate       = request.TradeDiscountRate;
        product.SpecialGoodsType        = request.SpecialGoodsType;
        product.LatestPurchasePrice     = request.LatestPurchasePrice;
        product.IsPromotionalGood       = request.IsPromotionalGood;

        var updated = await _repo.UpdateAsync(product, ct);
        _logger.LogInformation("Updated product {Id}", id);

        // Re-fetch với đầy đủ Include để DTO có tên hiển thị mới nhất (Category/ProductUnit/Warehouse/AccountSettings).
        var reloaded = await _repo.GetByIdAsync(id, ct) ?? updated;
        var dto = CreateProductUseCase.MapToDto(reloaded);
        await _broadcaster.ProductUpdatedAsync(dto, ct);
        return dto;
    }
}
