using Lamour.Application.Abstractions;
using Lamour.Application.Features.Products.Repositories;
using Lamour.Application.Features.SalesReturn.Dtos;
using Lamour.Application.Features.SalesReturn.Repositories;
using Lamour.Application.Features.Warehouse.Repositories;
using Lamour.Domain.Entities;
using Lamour.Domain.Exceptions;
using Microsoft.Extensions.Logging;

namespace Lamour.Application.Features.SalesReturn.UseCases;

// "Ghi sổ" — ảnh gương của UnconfirmSalesReturnUseCase: đảo chiều Held/Draft → Confirmed, CỘNG tồn
// kho (hàng trả lại = nhập kho). Không cần two-pass kiểm tra đủ tồn như Unconfirm vì cộng kho luôn
// thành công, không thể rơi về âm.
//
// 2026-09-10: cho phép Ghi sổ cả từ Draft, không chỉ Held — WPF (SalesReturnViewModel.ToggleConfirmAsync)
// giờ có luồng Bỏ ghi (Confirmed→Draft) rồi Ghi sổ lại NGAY (Draft→Confirmed) không cần Sửa/Cất qua
// Held trước — chỉ chặn khi ĐÃ Confirmed rồi (không thể ghi sổ lần 2).
public class ConfirmSalesReturnUseCase : IConfirmSalesReturnUseCase
{
    private readonly ISalesReturnRepository _repo;
    private readonly IProductRepository     _productRepo;
    private readonly IProductWarehouseStockRepository _stockRepo;
    private readonly IUnitOfWork            _uow;
    private readonly ILogger<ConfirmSalesReturnUseCase> _logger;

    public ConfirmSalesReturnUseCase(
        ISalesReturnRepository repo,
        IProductRepository productRepo,
        IProductWarehouseStockRepository stockRepo,
        IUnitOfWork uow,
        ILogger<ConfirmSalesReturnUseCase> logger)
    {
        _repo        = repo;
        _productRepo = productRepo;
        _stockRepo   = stockRepo;
        _uow         = uow;
        _logger      = logger;
    }

    public async Task<SalesReturnResponseDto> ExecuteAsync(int id, CancellationToken ct = default)
    {
        var salesReturn = await _repo.GetByIdTrackedAsync(id, ct)
            ?? throw new DomainException($"Sales return with id {id} not found.");

        if (salesReturn.Status == SalesReturnStatus.Confirmed)
            throw new DomainException("Chứng từ đã ở trạng thái đã ghi sổ rồi.");

        await _uow.BeginAsync(ct);
        try
        {
            foreach (var line in salesReturn.Lines)
            {
                var product = await _productRepo.GetByIdTrackedAsync(line.ProductId, ct);
                if (product is not null)
                {
                    product.StockQuantity += line.Quantity;
                    await _productRepo.UpdateAsync(product, ct);
                }
                await _stockRepo.AdjustQuantityAsync(line.ProductId, line.WarehouseId, line.Quantity, ct);
            }

            salesReturn.Status      = SalesReturnStatus.Confirmed;
            salesReturn.ConfirmedAt = DateTime.UtcNow;

            await _repo.UpdateAsync(salesReturn, ct);
            await _uow.CommitAsync(ct);

            _logger.LogInformation("Confirmed SalesReturn {Id} ({DocumentNumber}) — stock added for {LineCount} lines",
                id, salesReturn.DocumentNumber, salesReturn.Lines.Count);

            var updated = await _repo.GetByIdAsync(id, ct);
            return GetSalesReturnsUseCase.MapToDto(updated!);
        }
        catch
        {
            await _uow.RollbackAsync(ct);
            throw;
        }
    }
}
