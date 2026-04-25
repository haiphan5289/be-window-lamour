using Lamour.Application.Features.Products.Dtos;
using Lamour.Application.Features.Products.Repositories;
using Microsoft.Extensions.Logging;

namespace Lamour.Application.Features.Products.UseCases;

public class GetProductsUseCase : IGetProductsUseCase
{
    private readonly IProductRepository        _repo;
    private readonly ILogger<GetProductsUseCase> _logger;

    public GetProductsUseCase(IProductRepository repo, ILogger<GetProductsUseCase> logger)
    {
        _repo   = repo;
        _logger = logger;
    }

    public async Task<IEnumerable<ProductResponseDto>> ExecuteAsync(CancellationToken ct = default)
    {
        _logger.LogInformation("Fetching all products");
        var products = await _repo.GetAllAsync(ct);
        return products.Select(MapToDto);
    }

    private static ProductResponseDto MapToDto(Domain.Entities.Product p) => new()
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
