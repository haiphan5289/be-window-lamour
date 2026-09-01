using Lamour.Application.Abstractions;
using Lamour.Application.Features.Products.Repositories;
using Lamour.Application.Features.Sales.Dtos;
using Lamour.Application.Features.Sales.Repositories;
using Lamour.Application.Features.Warehouse.Repositories;
using Lamour.Domain.Entities;
using Lamour.Domain.Exceptions;
using Microsoft.Extensions.Logging;

namespace Lamour.Application.Features.Sales.UseCases;

public interface IHoldSalesOrderUseCase
{
    Task<SalesOrderResponseDto> ExecuteAsync(int id, CancellationToken ct = default);
}

// 2026-09-01: "Treo" giờ mang đúng nghĩa "chưa hoàn thành" — tồn kho không còn bị coi là đã bán
// trong lúc Treo. CreateSalesOrderUseCase/UpdateSalesOrderUseCase LUÔN trừ kho khi đưa đơn về
// Normal ("Ghi sổ" = hoàn thành); order chỉ có thể tới đây (Hold) khi đang ở Normal (guard bên
// dưới chặn Treo 1 đơn đã Treo sẵn) — nghĩa là tồn kho CHẮC CHẮN đã bị trừ trước đó, phải hoàn lại
// đúng số đó. Ngược lại, UpdateSalesOrderUseCase khi đưa 1 đơn ĐANG Treo về Normal (hoàn thành lần
// đầu) sẽ là nơi thật sự trừ kho — xem comment ở đó.
public class HoldSalesOrderUseCase : IHoldSalesOrderUseCase
{
    private readonly ISalesOrderRepository _repo;
    private readonly IProductRepository    _productRepo;
    private readonly IProductWarehouseStockRepository _stockRepo;
    private readonly IUnitOfWork           _uow;
    private readonly ILogger<HoldSalesOrderUseCase> _logger;

    public HoldSalesOrderUseCase(
        ISalesOrderRepository repo,
        IProductRepository productRepo,
        IProductWarehouseStockRepository stockRepo,
        IUnitOfWork uow,
        ILogger<HoldSalesOrderUseCase> logger)
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

        if (order.Status == SalesOrderStatus.Held)
            throw new DomainException("Chứng từ đã ở trạng thái Treo rồi.");

        await _uow.BeginAsync(ct);
        try
        {
            // Validate ALL lines trước (two-pass) để không hoàn tác dở dang nếu 1 dòng nào đó
            // không đủ tồn để hoàn — mirror UnconfirmSalesReturnUseCase/UnconfirmWarehouseReceiptUseCase.
            foreach (var line in order.Lines.Where(l => !l.IsPromotion))
            {
                var product = await _productRepo.GetByIdTrackedAsync(line.ProductId, ct);
                if (product is null || product.IsDepositProduct) continue; // "Đặt cọc" không phải hàng tồn kho thật

                if (product.StockQuantity < line.Quantity)
                    throw new DomainException(
                        $"Không thể Treo vì tồn kho hiện tại của hàng hóa '{product.Name}' không đủ để hoàn tác " +
                        "(đã phát sinh giao dịch xuất/nhập khác sau khi đơn này được Ghi sổ).");
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

            order.Status = SalesOrderStatus.Held;
            await _repo.UpdateAsync(order, ct);

            await _uow.CommitAsync(ct);

            _logger.LogInformation("SalesOrder {Id} marked as Held — stock reverted for {LineCount} lines", id, order.Lines.Count);

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
