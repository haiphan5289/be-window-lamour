using Lamour.Application.Abstractions;
using Lamour.Application.Features.Deposits.Repositories;
using Lamour.Application.Features.Products.Repositories;
using Lamour.Application.Features.Sales.Repositories;
using Lamour.Application.Features.Warehouse.Repositories;
using Lamour.Domain.Entities;
using Lamour.Domain.Exceptions;
using Microsoft.Extensions.Logging;

namespace Lamour.Application.Features.Sales.UseCases;

public class DeleteSalesOrderUseCase : IDeleteSalesOrderUseCase
{
    private readonly ISalesOrderRepository _repo;
    private readonly IProductRepository    _productRepo;
    private readonly IProductWarehouseStockRepository _stockRepo;
    private readonly IDepositRepository    _depositRepo;
    private readonly IUnitOfWork           _uow;
    private readonly ILogger<DeleteSalesOrderUseCase> _logger;

    public DeleteSalesOrderUseCase(
        ISalesOrderRepository repo,
        IProductRepository productRepo,
        IProductWarehouseStockRepository stockRepo,
        IDepositRepository depositRepo,
        IUnitOfWork uow,
        ILogger<DeleteSalesOrderUseCase> logger)
    {
        _repo        = repo;
        _productRepo = productRepo;
        _stockRepo   = stockRepo;
        _depositRepo = depositRepo;
        _uow         = uow;
        _logger      = logger;
    }

    public async Task ExecuteAsync(int id, CancellationToken ct = default)
    {
        var order = await _repo.GetByIdTrackedAsync(id, ct)
            ?? throw new DomainException($"Sales order with id {id} not found.");

        await _uow.BeginAsync(ct);
        try
        {
            await SalesOrderDepositHelper.GuardAndDeleteLinkedDepositAsync(_depositRepo, id, ct);

            // Restore stock for non-promotion lines — CHỈ khi đơn đang Normal (đã từng trừ kho
            // thật lúc Create/Update). Đơn đang Treo chưa từng trừ kho (xem HoldSalesOrderUseCase/
            // CreateSalesOrderUseCase — 2026-09-01) nên xóa 1 đơn Treo không cần hoàn tác gì, hoàn
            // vô điều kiện như trước sẽ cộng dư tồn kho không có thật.
            if (order.Status == SalesOrderStatus.Normal)
            {
                foreach (var line in order.Lines.Where(l => !l.IsPromotion))
                {
                    var product = await _productRepo.GetByIdTrackedAsync(line.ProductId, ct);
                    if (product is not null && product.IsDepositProduct)
                        continue; // "Đặt cọc" không phải hàng tồn kho thật

                    if (product is not null)
                    {
                        product.StockQuantity += line.Quantity;
                        await _productRepo.UpdateAsync(product, ct);
                    }
                    await _stockRepo.AdjustQuantityAsync(line.ProductId, line.WarehouseId!.Value, line.Quantity, ct);
                }
            }

            await _repo.DeleteAsync(order, ct);
            await _uow.CommitAsync(ct);

            _logger.LogInformation("Deleted SalesOrder {Id} ({DocumentNumber})", id, order.DocumentNumber);
        }
        catch
        {
            await _uow.RollbackAsync(ct);
            throw;
        }
    }
}
