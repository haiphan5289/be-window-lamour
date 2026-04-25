using Lamour.Application.Features.Products.Dtos;
using Lamour.Application.Features.Products.Repositories;
using Lamour.Domain.Entities;
using Lamour.Domain.Exceptions;
using Microsoft.Extensions.Logging;

namespace Lamour.Application.Features.Products.UseCases;

public class CreateProductUseCase : ICreateProductUseCase
{
    private readonly IProductRepository          _repo;
    private readonly ILogger<CreateProductUseCase> _logger;

    public CreateProductUseCase(IProductRepository repo, ILogger<CreateProductUseCase> logger)
    {
        _repo   = repo;
        _logger = logger;
    }

    public async Task<ProductResponseDto> ExecuteAsync(CreateProductRequestDto request, CancellationToken ct = default)
    {
        if (string.IsNullOrWhiteSpace(request.Name))
            throw new DomainException("Product name is required.");
        if (string.IsNullOrWhiteSpace(request.Category))
            throw new DomainException("Product category is required.");
        if (request.CostPrice <= 0)
            throw new DomainException("Cost price must be greater than zero.");
        if (request.SellingPrice <= 0)
            throw new DomainException("Selling price must be greater than zero.");

        if (!string.IsNullOrWhiteSpace(request.Code) && await _repo.CodeExistsAsync(request.Code, ct: ct))
            throw new DomainException($"Product with code '{request.Code}' already exists.");

        var product = new Product
        {
            Code          = request.Code.Trim(),
            Name          = request.Name.Trim(),
            Category      = request.Category.Trim(),
            Unit          = request.Unit,
            CostPrice     = request.CostPrice,
            SellingPrice  = request.SellingPrice,
            StockQuantity = request.StockQuantity,
            IsActive      = request.IsActive,
        };

        var created = await _repo.AddAsync(product, ct);
        _logger.LogInformation("Created product {Id} '{Name}'", created.Id, created.Name);

        return MapToDto(created);
    }

    private static ProductResponseDto MapToDto(Product p) => new()
    {
        Id            = p.Id,
        Code          = p.Code,
        Name          = p.Name,
        Category      = p.Category,
        Unit          = p.Unit,
        CostPrice     = p.CostPrice,
        SellingPrice  = p.SellingPrice,
        StockQuantity = p.StockQuantity,
        IsActive      = p.IsActive,
    };
}
