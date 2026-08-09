using Lamour.Application.Abstractions;
using Lamour.Application.Features.Warehouses.Repositories;
using Lamour.Domain.Exceptions;
using Microsoft.Extensions.Logging;

namespace Lamour.Application.Features.Warehouses.UseCases;

public class DeleteWarehouseUseCase : IDeleteWarehouseUseCase
{
    private readonly IWarehouseRepository _repo;
    private readonly INotificationBroadcaster _broadcaster;
    private readonly ILogger<DeleteWarehouseUseCase> _logger;

    public DeleteWarehouseUseCase(IWarehouseRepository repo, INotificationBroadcaster broadcaster, ILogger<DeleteWarehouseUseCase> logger)
    {
        _repo        = repo;
        _broadcaster = broadcaster;
        _logger      = logger;
    }

    public async Task ExecuteAsync(int id, CancellationToken ct = default)
    {
        var warehouse = await _repo.GetByIdAsync(id, ct)
            ?? throw new NotFoundException($"Warehouse {id} not found.");

        await _repo.DeleteAsync(warehouse, ct);
        _logger.LogInformation("Deleted warehouse {Id}", id);

        await _broadcaster.WarehouseDeletedAsync(id, ct);
    }
}
