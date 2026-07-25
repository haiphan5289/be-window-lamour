using Lamour.Application.Abstractions;
using Lamour.Application.Features.Categories.Repositories;
using Lamour.Application.Features.Products.Dtos;
using Lamour.Application.Features.Products.Repositories;
using Lamour.Domain.Entities;
using Lamour.Domain.Enums;
using Lamour.Domain.Exceptions;
using Microsoft.Extensions.Logging;

namespace Lamour.Application.Features.Products.UseCases;

public class CreateProductUseCase : ICreateProductUseCase
{
    private readonly IProductRepository          _repo;
    private readonly ICategoryRepository         _categoryRepo;
    private readonly INotificationBroadcaster    _broadcaster;
    private readonly ILogger<CreateProductUseCase> _logger;

    public CreateProductUseCase(
        IProductRepository repo,
        ICategoryRepository categoryRepo,
        INotificationBroadcaster broadcaster,
        ILogger<CreateProductUseCase> logger)
    {
        _repo         = repo;
        _categoryRepo = categoryRepo;
        _broadcaster  = broadcaster;
        _logger       = logger;
    }

    public async Task<ProductResponseDto> ExecuteAsync(CreateProductRequestDto request, CancellationToken ct = default)
    {
        if (string.IsNullOrWhiteSpace(request.Name))
            throw new DomainException("Product name is required.");

        var category = await _categoryRepo.GetByIdAsync(request.CategoryId, ct)
            ?? throw new DomainException($"Danh mục với id {request.CategoryId} không tồn tại.");

        if (!string.IsNullOrWhiteSpace(request.Code) && await _repo.CodeExistsAsync(request.Code, ct: ct))
            throw new DomainException($"Product with code '{request.Code}' already exists.");

        var product = new Product
        {
            Code             = request.Code.Trim(),
            Name             = request.Name.Trim(),
            CategoryId       = request.CategoryId,
            Unit             = request.Unit,
            CostPrice        = request.CostPrice,
            SellingPrice     = request.SellingPrice,
            StockQuantity    = request.StockQuantity,
            IsActive         = request.IsActive,
            VatRate          = ParseVatRate(request.VatRate),
            TaxReductionType = ParseTaxReductionStatus(request.TaxReductionType),
            ImportTaxRate    = request.ImportTaxRate,
            ExportTaxRate    = request.ExportTaxRate,
            ExciseTaxGroup   = request.ExciseTaxGroup,
        };

        var created = await _repo.AddAsync(product, ct);
        created.Category = category;
        _logger.LogInformation("Created product {Id} '{Name}'", created.Id, created.Name);

        var dto = MapToDto(created);
        await _broadcaster.ProductCreatedAsync(dto, ct);
        return dto;
    }

    internal static VatRateType? ParseVatRate(string? value) =>
        Enum.TryParse<VatRateType>(value, out var result) ? result : null;

    internal static TaxReductionStatus? ParseTaxReductionStatus(string? value) =>
        Enum.TryParse<TaxReductionStatus>(value, out var result) ? result : null;

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
    };
}
