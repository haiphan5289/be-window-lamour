using Lamour.Application.Abstractions;
using Lamour.Application.Features.Products.Dtos;
using Lamour.Application.Features.Products.Repositories;
using Lamour.Domain.Exceptions;
using Microsoft.Extensions.Logging;

namespace Lamour.Application.Features.Products.UseCases;

public class UpdateProductUseCase : IUpdateProductUseCase
{
    private readonly IProductRepository          _repo;
    private readonly INotificationBroadcaster    _broadcaster;
    private readonly ILogger<UpdateProductUseCase> _logger;

    public UpdateProductUseCase(IProductRepository repo, INotificationBroadcaster broadcaster, ILogger<UpdateProductUseCase> logger)
    {
        _repo        = repo;
        _broadcaster = broadcaster;
        _logger      = logger;
    }

    public async Task<ProductResponseDto> ExecuteAsync(int id, UpdateProductRequestDto request, CancellationToken ct = default)
    {
        var product = await _repo.GetByIdAsync(id, ct)
            ?? throw new NotFoundException($"Product {id} not found.");

        if (string.IsNullOrWhiteSpace(request.Name))
            throw new DomainException("Product name is required.");
        if (string.IsNullOrWhiteSpace(request.Category))
            throw new DomainException("Product category is required.");
        if (request.CostPrice <= 0)
            throw new DomainException("Cost price must be greater than zero.");
        if (request.SellingPrice <= 0)
            throw new DomainException("Selling price must be greater than zero.");

        if (!string.IsNullOrWhiteSpace(request.Code) && await _repo.CodeExistsAsync(request.Code, excludeId: id, ct: ct))
            throw new DomainException($"Product with code '{request.Code}' already exists.");

        product.Code             = request.Code.Trim();
        product.Name             = request.Name.Trim();
        product.Category         = request.Category.Trim();
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
        _logger.LogInformation("Updated product {Id}", id);

        var dto = CreateProductUseCase.MapToDto(updated);
        await _broadcaster.ProductUpdatedAsync(dto, ct);
        return dto;
    }
}
