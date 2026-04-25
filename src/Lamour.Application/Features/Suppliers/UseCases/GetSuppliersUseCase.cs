using Lamour.Application.Features.Suppliers.Dtos;
using Lamour.Application.Features.Suppliers.Repositories;
using Microsoft.Extensions.Logging;

namespace Lamour.Application.Features.Suppliers.UseCases;

public class GetSuppliersUseCase : IGetSuppliersUseCase
{
    private readonly ISupplierRepository _repo;
    private readonly ILogger<GetSuppliersUseCase> _logger;

    public GetSuppliersUseCase(ISupplierRepository repo, ILogger<GetSuppliersUseCase> logger)
    {
        _repo   = repo;
        _logger = logger;
    }

    public async Task<IEnumerable<SupplierResponseDto>> ExecuteAsync(CancellationToken ct = default)
    {
        _logger.LogInformation("Fetching all suppliers");
        var suppliers = await _repo.GetAllAsync(ct);
        return suppliers.Select(s => new SupplierResponseDto
        {
            Id             = s.Id,
            Code           = s.Code,
            Name           = s.Name,
            Address        = s.Address,
            Group          = s.Group,
            TaxCode        = s.TaxCode,
            Phone          = s.Phone,
            IsStopTracking = s.IsStopTracking,
        });
    }
}
