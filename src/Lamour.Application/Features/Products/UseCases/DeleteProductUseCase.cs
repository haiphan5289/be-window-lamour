using Lamour.Application.Abstractions;
using Lamour.Application.Features.Products.Repositories;
using Lamour.Domain.Exceptions;
using Microsoft.Extensions.Logging;

namespace Lamour.Application.Features.Products.UseCases;

public class DeleteProductUseCase : IDeleteProductUseCase
{
    private readonly IProductRepository         _repo;
    private readonly INotificationBroadcaster   _broadcaster;
    private readonly ILogger<DeleteProductUseCase> _logger;

    public DeleteProductUseCase(IProductRepository repo, INotificationBroadcaster broadcaster, ILogger<DeleteProductUseCase> logger)
    {
        _repo        = repo;
        _broadcaster = broadcaster;
        _logger      = logger;
    }

    public async Task ExecuteAsync(int id, CancellationToken ct = default)
    {
        var product = await _repo.GetByIdAsync(id, ct)
            ?? throw new NotFoundException($"Product {id} not found.");

        if (await _repo.IsInUseAsync(id, ct))
            throw new DomainException($"Không thể xóa sản phẩm '{product.Name}' vì đang có trong đơn bán hàng, phiếu nhập kho hoặc hàng bán bị trả lại.");

        await _repo.DeleteAsync(product, ct);
        _logger.LogInformation("Deleted product {Id}", id);

        await _broadcaster.ProductDeletedAsync(id, ct);
    }
}
