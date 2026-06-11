using Lamour.Application.Features.Sales.Dtos;
using Lamour.Application.Features.Sales.Repositories;
using Lamour.Domain.Entities;
using Lamour.Domain.Exceptions;
using Microsoft.Extensions.Logging;

namespace Lamour.Application.Features.Sales.UseCases;

public interface IConfirmSalesOrderUseCase
{
    Task<SalesOrderResponseDto> ExecuteAsync(int id, CancellationToken ct = default);
}

public class ConfirmSalesOrderUseCase : IConfirmSalesOrderUseCase
{
    private readonly ISalesOrderRepository _repo;
    private readonly ILogger<ConfirmSalesOrderUseCase> _logger;

    public ConfirmSalesOrderUseCase(ISalesOrderRepository repo, ILogger<ConfirmSalesOrderUseCase> logger)
    {
        _repo   = repo;
        _logger = logger;
    }

    public async Task<SalesOrderResponseDto> ExecuteAsync(int id, CancellationToken ct = default)
    {
        var order = await _repo.GetByIdTrackedAsync(id, ct)
            ?? throw new DomainException($"Sales order with id {id} not found.");

        if (order.Status == SalesOrderStatus.Confirmed)
            throw new DomainException("Đơn hàng đã được xác nhận trước đó.");

        order.Status = SalesOrderStatus.Confirmed;
        await _repo.UpdateAsync(order, ct);

        _logger.LogInformation("SalesOrder {Id} confirmed", id);

        var updated = await _repo.GetByIdAsync(id, ct);
        return GetSalesOrdersUseCase.MapToDto(updated!);
    }
}
