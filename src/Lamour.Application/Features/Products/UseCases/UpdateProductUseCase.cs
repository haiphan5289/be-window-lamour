using Lamour.Application.Abstractions;
using Lamour.Application.Features.Categories.Repositories;
using Lamour.Application.Features.Products.Dtos;
using Lamour.Application.Features.Products.Repositories;
using Lamour.Domain.Exceptions;
using Microsoft.Extensions.Logging;

namespace Lamour.Application.Features.Products.UseCases;

public class UpdateProductUseCase : IUpdateProductUseCase
{
    private readonly IProductRepository          _repo;
    private readonly ICategoryRepository         _categoryRepo;
    private readonly INotificationBroadcaster    _broadcaster;
    private readonly ILogger<UpdateProductUseCase> _logger;

    public UpdateProductUseCase(
        IProductRepository repo,
        ICategoryRepository categoryRepo,
        INotificationBroadcaster broadcaster,
        ILogger<UpdateProductUseCase> logger)
    {
        _repo         = repo;
        _categoryRepo = categoryRepo;
        _broadcaster  = broadcaster;
        _logger       = logger;
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

        product.Code             = request.Code.Trim();
        product.Name             = request.Name.Trim();
        product.CategoryId       = request.CategoryId;
        product.Unit             = request.Unit;
        product.CostPrice        = request.CostPrice;
        product.SellingPrice     = request.SellingPrice;
        product.StockQuantity    = request.StockQuantity;
        product.IsActive         = request.IsActive;
        product.VatRate          = CreateProductUseCase.ParseVatRate(request.VatRate);
        product.TaxReductionType = CreateProductUseCase.ParseTaxReductionStatus(request.TaxReductionType);
        product.ImportTaxRate    = request.ImportTaxRate;
        product.ExportTaxRate    = request.ExportTaxRate;
        product.ExciseTaxGroup   = request.ExciseTaxGroup;

        var updated = await _repo.UpdateAsync(product, ct);
        updated.Category = category; // ensure DTO reflects the (possibly new) category name, not a stale/no-op-tracked navigation
        _logger.LogInformation("Updated product {Id}", id);

        var dto = CreateProductUseCase.MapToDto(updated);
        await _broadcaster.ProductUpdatedAsync(dto, ct);
        return dto;
    }
}
