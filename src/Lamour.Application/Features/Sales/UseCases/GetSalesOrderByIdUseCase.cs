using Lamour.Application.Features.Sales.Dtos;
using Lamour.Application.Features.Sales.Repositories;
using Microsoft.Extensions.Logging;

namespace Lamour.Application.Features.Sales.UseCases;

public class GetSalesOrderByIdUseCase : IGetSalesOrderByIdUseCase
{
    private readonly ISalesOrderRepository _repo;
    private readonly ILogger<GetSalesOrderByIdUseCase> _logger;

    public GetSalesOrderByIdUseCase(ISalesOrderRepository repo, ILogger<GetSalesOrderByIdUseCase> logger)
    {
        _repo   = repo;
        _logger = logger;
    }

    public async Task<SalesOrderResponseDto?> ExecuteAsync(int id, CancellationToken ct = default)
    {
        _logger.LogInformation("Fetching sales order {Id}", id);
        var order = await _repo.GetByIdAsync(id, ct);
        return order is null ? null : GetSalesOrdersUseCase.MapToDto(order);
    }
}
