using Lamour.Application.Abstractions;
using Lamour.Application.Features.Products.Repositories;
using Lamour.Application.Features.Sales.Dtos;
using Lamour.Application.Features.Sales.Repositories;
using Lamour.Application.Features.Warehouse.Repositories;
using Lamour.Domain.Entities;
using Lamour.Domain.Exceptions;
using Microsoft.Extensions.Logging;

namespace Lamour.Application.Features.Sales.UseCases;

// "Bỏ ghi" — mirror HoldSalesOrderUseCase gần như nguyên vẹn (cùng shape: guard trạng thái, 2-pass
// hoàn tồn kho) nhưng target Status.Draft thay vì Held — đây là 2 khái niệm khác nhau dù cơ chế
// hoàn tồn kho giống hệt (xem comment enum SalesOrderStatus). "Treo" dùng cho đơn CHƯA hoàn chỉnh;
// "Bỏ ghi" dùng để mở khóa sửa 1 đơn ĐÃ Ghi sổ (Normal) — chỉ đảo trạng thái + tồn kho, KHÔNG tự mở
// khóa form (xem SalesOrderViewModel.IsReadOnly/CanEdit/Edit() phía WPF).
public class UnconfirmSalesOrderUseCase : IUnconfirmSalesOrderUseCase
{
    private readonly ISalesOrderRepository _repo;
    private readonly IProductRepository    _productRepo;
    private readonly IProductWarehouseStockRepository _stockRepo;
    private readonly IUnitOfWork           _uow;
    private readonly ILogger<UnconfirmSalesOrderUseCase> _logger;

    public UnconfirmSalesOrderUseCase(
        ISalesOrderRepository repo,
        IProductRepository productRepo,
        IProductWarehouseStockRepository stockRepo,
        IUnitOfWork uow,
        ILogger<UnconfirmSalesOrderUseCase> logger)
    {
        _repo        = repo;
        _productRepo = productRepo;
        _stockRepo   = stockRepo;
        _uow         = uow;
        _logger      = logger;
    }

    public async Task<SalesOrderResponseDto> ExecuteAsync(int id, CancellationToken ct = default)
    {
        var order = await _repo.GetByIdTrackedAsync(id, ct)
            ?? throw new DomainException($"Sales order with id {id} not found.");

        if (order.Status != SalesOrderStatus.Normal)
            throw new DomainException("Chứng từ chưa ở trạng thái đã ghi sổ, không thể bỏ ghi.");

        await _uow.BeginAsync(ct);
        try
        {
            // Validate ALL lines trước (two-pass) để không hoàn tác dở dang nếu 1 dòng nào đó
            // không đủ tồn để hoàn — mirror HoldSalesOrderUseCase/UnconfirmSalesReturnUseCase.
            foreach (var line in order.Lines.Where(l => !l.IsPromotion))
            {
                var product = await _productRepo.GetByIdTrackedAsync(line.ProductId, ct);
                if (product is null || product.IsDepositProduct) continue; // "Đặt cọc" không phải hàng tồn kho thật

                if (product.StockQuantity < line.Quantity)
                    throw new DomainException(
                        $"Không thể bỏ ghi vì tồn kho hiện tại của hàng hóa '{product.Name}' không đủ để " +
                        "hoàn tác (đã phát sinh giao dịch xuất/nhập khác sau khi đơn này được Ghi sổ).");
            }

            foreach (var line in order.Lines.Where(l => !l.IsPromotion))
            {
                var product = await _productRepo.GetByIdTrackedAsync(line.ProductId, ct);
                if (product is not null && product.IsDepositProduct)
                    continue;

                if (product is not null)
                {
                    product.StockQuantity += line.Quantity;
                    await _productRepo.UpdateAsync(product, ct);
                }
                await _stockRepo.AdjustQuantityAsync(line.ProductId, line.WarehouseId!.Value, line.Quantity, ct);
            }

            // 2026-09-11: gộp "Nháp" và "Treo" thành 1 trạng thái duy nhất "Treo" (Held) — mirror
            // SalesReturn cùng ngày. "Bỏ ghi" giờ luôn đưa đơn về Held thay vì Draft.
            order.Status = SalesOrderStatus.Held;
            await _repo.UpdateAsync(order, ct);

            await _uow.CommitAsync(ct);

            _logger.LogInformation("Unconfirmed SalesOrder {Id} ({DocumentNumber}) — stock reverted for {LineCount} lines",
                id, order.DocumentNumber, order.Lines.Count);

            var updated = await _repo.GetByIdAsync(id, ct);
            return GetSalesOrdersUseCase.MapToDto(updated!);
        }
        catch
        {
            await _uow.RollbackAsync(ct);
            throw;
        }
    }
}
