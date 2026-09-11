using Lamour.Application.Abstractions;
using Lamour.Application.Features.Products.Repositories;
using Lamour.Application.Features.SalesReturn.Dtos;
using Lamour.Application.Features.SalesReturn.Repositories;
using Lamour.Application.Features.Warehouse.Repositories;
using Lamour.Domain.Entities;
using Lamour.Domain.Exceptions;
using Microsoft.Extensions.Logging;

namespace Lamour.Application.Features.SalesReturn.UseCases;

// "Bỏ ghi" — tái kích hoạt vòng đời Nháp/Confirmed đã tưởng bỏ hẳn (2026-09-07): trả chứng từ về
// Draft + hoàn tác đúng số tồn kho đã cộng lúc Create/Update gần nhất. Mirror
// WarehouseReceipts/UseCases/UnconfirmWarehouseReceiptUseCase.cs (workflow "Ghi sổ/Bỏ ghi" đã chạy
// ổn ở đó) nhưng dùng lại đúng kiểu kiểm tra tồn kho (product.StockQuantity, IUnitOfWork) như
// UpdateSalesReturnUseCase/DeleteSalesReturnUseCase trong cùng feature này, để nhất quán trong nội
// bộ SalesReturn thay vì trộn 2 convention khác nhau.
public class UnconfirmSalesReturnUseCase : IUnconfirmSalesReturnUseCase
{
    private readonly ISalesReturnRepository _repo;
    private readonly IProductRepository     _productRepo;
    private readonly IProductWarehouseStockRepository _stockRepo;
    private readonly IUnitOfWork            _uow;
    private readonly ILogger<UnconfirmSalesReturnUseCase> _logger;

    public UnconfirmSalesReturnUseCase(
        ISalesReturnRepository repo,
        IProductRepository productRepo,
        IProductWarehouseStockRepository stockRepo,
        IUnitOfWork uow,
        ILogger<UnconfirmSalesReturnUseCase> logger)
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

        if (salesReturn.Status != SalesReturnStatus.Confirmed)
            throw new DomainException("Chứng từ chưa ở trạng thái đã ghi sổ, không thể bỏ ghi.");

        await _uow.BeginAsync(ct);
        try
        {
            // Hàng trả lại = đã nhập kho lúc Ghi sổ, nên "bỏ ghi" = hoàn tác = TRỪ tồn kho — kiểm
            // tra đủ tồn trước (đã xuất bán tiếp sau khi ghi sổ thì không cho bỏ ghi), hai lượt
            // đúng pattern DeleteSalesReturnUseCase/UpdateSalesReturnUseCase.
            foreach (var line in salesReturn.Lines)
            {
                var product = await _productRepo.GetByIdTrackedAsync(line.ProductId, ct)
                    ?? throw new DomainException($"Product with id {line.ProductId} not found.");
                if (product.StockQuantity < line.Quantity)
                    throw new DomainException(
                        $"Không thể bỏ ghi vì tồn kho hiện tại của hàng hóa '{product.Name}' không đủ để " +
                        "hoàn tác số lượng đã nhập lại kho (đã phát sinh giao dịch xuất kho sau khi ghi sổ).");
            }

            foreach (var line in salesReturn.Lines)
            {
                var product = await _productRepo.GetByIdTrackedAsync(line.ProductId, ct);
                if (product is not null)
                {
                    product.StockQuantity -= line.Quantity;
                    await _productRepo.UpdateAsync(product, ct);
                }
                await _stockRepo.AdjustQuantityAsync(line.ProductId, line.WarehouseId, -line.Quantity, ct);
            }

            // 2026-09-11: gộp "Nháp" và "Treo" thành 1 trạng thái duy nhất "Treo" (Held) — theo yêu
            // cầu, sau khi "Cất" đã luôn Ghi sổ ngay thì Held/Draft không còn khác biệt gì về nghiệp
            // vụ (đã xác nhận qua rà code: mọi guard chỉ dựa vào Confirmed, không nơi nào phân biệt
            // Held vs Draft). "Bỏ ghi" giờ luôn đưa về Held thay vì Draft.
            salesReturn.Status      = SalesReturnStatus.Held;
            salesReturn.ConfirmedAt = null;

            await _repo.UpdateAsync(salesReturn, ct);
            await _uow.CommitAsync(ct);

            _logger.LogInformation("Unconfirmed SalesReturn {Id} ({DocumentNumber}) — stock reverted for {LineCount} lines",
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
