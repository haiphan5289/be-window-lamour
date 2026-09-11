using Lamour.Application.Abstractions;
using Lamour.Application.Features.Products.Repositories;
using Lamour.Application.Features.Sales.Dtos;
using Lamour.Application.Features.Sales.Repositories;
using Lamour.Application.Features.Warehouse.Repositories;
using Lamour.Domain.Entities;
using Lamour.Domain.Exceptions;
using Microsoft.Extensions.Logging;

namespace Lamour.Application.Features.Sales.UseCases;

// "Ghi sổ" — ảnh gương của UnconfirmSalesOrderUseCase: đảo chiều Held/Draft → Normal, TRỪ tồn kho
// (mirror khối trừ kho vốn nằm trong CreateSalesOrderUseCase/UpdateSalesOrderUseCase trước
// 2026-09-10, nay tách thành hành động riêng vì "Cất" không còn tự Ghi sổ).
//
// 2026-09-10: cho phép Ghi sổ cả từ Draft, không chỉ Held — mirror ConfirmSalesReturnUseCase cùng
// ngày. WPF (SalesOrderViewModel.ToggleConfirmAsync) có luồng Bỏ ghi (Normal→Draft) rồi Ghi sổ lại
// NGAY (Draft→Normal) không cần Sửa/Cất qua Held trước — chỉ chặn khi ĐÃ Normal rồi.
public class ConfirmSalesOrderUseCase : IConfirmSalesOrderUseCase
{
    private readonly ISalesOrderRepository _repo;
    private readonly IProductRepository    _productRepo;
    private readonly IProductWarehouseStockRepository _stockRepo;
    private readonly IUnitOfWork           _uow;
    private readonly ILogger<ConfirmSalesOrderUseCase> _logger;

    public ConfirmSalesOrderUseCase(
        ISalesOrderRepository repo,
        IProductRepository productRepo,
        IProductWarehouseStockRepository stockRepo,
        IUnitOfWork uow,
        ILogger<ConfirmSalesOrderUseCase> logger)
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

        if (order.Status == SalesOrderStatus.Normal)
            throw new DomainException("Chứng từ đã ở trạng thái đã ghi sổ rồi.");

        await _uow.BeginAsync(ct);
        try
        {
            // Validate ALL lines trước (two-pass) — mirror UnconfirmSalesOrderUseCase/
            // HoldSalesOrderUseCase — tránh trừ kho dở dang nếu 1 dòng nào đó không đủ tồn.
            var stockErrors = new List<string>();
            foreach (var line in order.Lines.Where(l => !l.IsPromotion))
            {
                var product = await _productRepo.GetByIdTrackedAsync(line.ProductId, ct);
                if (product is null || product.IsDepositProduct) continue; // "Đặt cọc" không phải hàng tồn kho thật

                var availableQty = await _stockRepo.GetQuantityAsync(line.ProductId, line.WarehouseId!.Value, ct);
                if (availableQty < line.Quantity)
                    stockErrors.Add($"• {product.Name}: kho có {availableQty}, cần {line.Quantity}");
            }

            if (stockErrors.Count > 0)
                throw new DomainException("Các sản phẩm không đủ tồn kho:\n" + string.Join("\n", stockErrors));

            foreach (var line in order.Lines.Where(l => !l.IsPromotion))
            {
                var product = await _productRepo.GetByIdTrackedAsync(line.ProductId, ct);
                if (product is not null && product.IsDepositProduct)
                    continue;

                if (product is not null)
                {
                    product.StockQuantity -= line.Quantity;
                    await _productRepo.UpdateAsync(product, ct);
                }
                await _stockRepo.AdjustQuantityAsync(line.ProductId, line.WarehouseId!.Value, -line.Quantity, ct);
            }

            order.Status = SalesOrderStatus.Normal;
            await _repo.UpdateAsync(order, ct);

            await _uow.CommitAsync(ct);

            _logger.LogInformation("Confirmed SalesOrder {Id} ({DocumentNumber}) — stock deducted for {LineCount} lines",
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
