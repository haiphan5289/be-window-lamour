using Lamour.Application.Abstractions;
using Lamour.Application.Features.Products.Dtos;
using Lamour.Application.Features.Products.Repositories;
using Lamour.Domain.Entities;
using Lamour.Domain.Exceptions;
using Microsoft.Extensions.Logging;

namespace Lamour.Application.Features.Products.UseCases;

public class DuplicateProductUseCase : IDuplicateProductUseCase
{
    private readonly IProductRepository             _repo;
    private readonly INotificationBroadcaster       _broadcaster;
    private readonly ILogger<DuplicateProductUseCase> _logger;

    public DuplicateProductUseCase(IProductRepository repo, INotificationBroadcaster broadcaster, ILogger<DuplicateProductUseCase> logger)
    {
        _repo        = repo;
        _broadcaster = broadcaster;
        _logger      = logger;
    }

    public async Task<ProductResponseDto> ExecuteAsync(int id, CancellationToken ct = default)
    {
        var source = await _repo.GetByIdAsync(id, ct)
            ?? throw new NotFoundException($"Product {id} not found.");

        var newCode = string.IsNullOrWhiteSpace(source.Code) ? string.Empty : source.Code + "_COPY";

        if (!string.IsNullOrWhiteSpace(newCode) && await _repo.CodeExistsAsync(newCode, ct: ct))
            throw new DomainException($"Product with code '{newCode}' already exists.");

        var copy = new Product
        {
            Code             = newCode,
            Name             = source.Name,
            Category         = source.Category,
            Unit             = source.Unit,
            CostPrice        = source.CostPrice,
            SellingPrice     = source.SellingPrice,
            StockQuantity    = source.StockQuantity,
            IsActive         = source.IsActive,
            VatRate          = source.VatRate,
            TaxReductionType = source.TaxReductionType,
            ImportTaxRate    = source.ImportTaxRate,
            ExportTaxRate    = source.ExportTaxRate,
            ExciseTaxGroup   = source.ExciseTaxGroup,
        };

        var created = await _repo.AddAsync(copy, ct);
        _logger.LogInformation("Duplicated product {SourceId} → {NewId}", id, created.Id);

        var dto = CreateProductUseCase.MapToDto(created);
        await _broadcaster.ProductCreatedAsync(dto, ct);
        return dto;
    }
}
