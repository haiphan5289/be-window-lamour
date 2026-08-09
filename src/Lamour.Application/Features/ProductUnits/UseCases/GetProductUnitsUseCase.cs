using Lamour.Application.Features.ProductUnits.Dtos;
using Lamour.Application.Features.ProductUnits.Repositories;
using Lamour.Domain.Entities;
using Microsoft.Extensions.Logging;

namespace Lamour.Application.Features.ProductUnits.UseCases;

public class GetProductUnitsUseCase : IGetProductUnitsUseCase
{
    private readonly IProductUnitRepository _repo;
    private readonly ILogger<GetProductUnitsUseCase> _logger;

    public GetProductUnitsUseCase(IProductUnitRepository repo, ILogger<GetProductUnitsUseCase> logger)
    {
        _repo   = repo;
        _logger = logger;
    }

    public async Task<IEnumerable<ProductUnitResponseDto>> ExecuteAsync(CancellationToken ct = default)
    {
        _logger.LogInformation("Fetching all product units");
        var units = await _repo.GetAllAsync(ct);
        return units.Select(MapToDto);
    }

    internal static ProductUnitResponseDto MapToDto(ProductUnit u) => new()
    {
        Id   = u.Id,
        Name = u.Name,
    };
}
