using Lamour.Application.Features.Sales.Dtos;
using Lamour.Application.Features.Sales.Repositories;
using Lamour.Domain.Entities;
using Lamour.Domain.Exceptions;
using Microsoft.Extensions.Logging;

namespace Lamour.Application.Features.Sales.UseCases;

public interface IHoldSalesOrderUseCase
{
    Task<SalesOrderResponseDto> ExecuteAsync(int id, CancellationToken ct = default);
}

public class HoldSalesOrderUseCase : IHoldSalesOrderUseCase
{
    private readonly ISalesOrderRepository _repo;
    private readonly ILogger<HoldSalesOrderUseCase> _logger;

    public HoldSalesOrderUseCase(ISalesOrderRepository repo, ILogger<HoldSalesOrderUseCase> logger)
    {
        _repo   = repo;
        _logger = logger;
    }

    public async Task<SalesOrderResponseDto> ExecuteAsync(int id, CancellationToken ct = default)
    {
        var order = await _repo.GetByIdTrackedAsync(id, ct)
            ?? throw new DomainException($"Sales order with id {id} not found.");

        order.Status = SalesOrderStatus.Held;
        await _repo.UpdateAsync(order, ct);

        _logger.LogInformation("SalesOrder {Id} marked as Held", id);

        var updated = await _repo.GetByIdAsync(id, ct);
        return GetSalesOrdersUseCase.MapToDto(updated!);
    }
}
