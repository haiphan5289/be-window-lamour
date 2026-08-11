using Lamour.Application.Abstractions;
using Lamour.Application.Features.ProductUnits.Repositories;
using Lamour.Domain.Exceptions;
using Microsoft.Extensions.Logging;

namespace Lamour.Application.Features.ProductUnits.UseCases;

public class DeleteProductUnitUseCase : IDeleteProductUnitUseCase
{
    private readonly IProductUnitRepository _repo;
    private readonly INotificationBroadcaster _broadcaster;
    private readonly ILogger<DeleteProductUnitUseCase> _logger;

    public DeleteProductUnitUseCase(IProductUnitRepository repo, INotificationBroadcaster broadcaster, ILogger<DeleteProductUnitUseCase> logger)
    {
        _repo        = repo;
        _broadcaster = broadcaster;
        _logger      = logger;
    }

    public async Task ExecuteAsync(int id, CancellationToken ct = default)
    {
        var unit = await _repo.GetByIdAsync(id, ct)
            ?? throw new NotFoundException($"Product unit {id} not found.");

        if (await _repo.IsInUseAsync(id, ct))
            throw new DomainException($"Đơn vị tính '{unit.Name}' đang được sản phẩm sử dụng, không thể xoá.");

        await _repo.DeleteAsync(unit, ct);
        _logger.LogInformation("Deleted product unit {Id}", id);

        await _broadcaster.ProductUnitDeletedAsync(id, ct);
    }
}
