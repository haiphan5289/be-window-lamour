using Lamour.Application.Abstractions;
using Lamour.Application.Features.Products.Repositories;
using Lamour.Application.Features.SalesReturn.Repositories;
using Lamour.Application.Features.Warehouse.Repositories;
using Lamour.Domain.Entities;
using Lamour.Domain.Exceptions;
using Microsoft.Extensions.Logging;

namespace Lamour.Application.Features.SalesReturn.UseCases;

public class DeleteSalesReturnUseCase : IDeleteSalesReturnUseCase
{
    private readonly ISalesReturnRepository _repo;
    private readonly IProductRepository     _productRepo;
    private readonly IProductWarehouseStockRepository _stockRepo;
    private readonly IUnitOfWork            _uow;
    private readonly ILogger<DeleteSalesReturnUseCase> _logger;

    public DeleteSalesReturnUseCase(
        ISalesReturnRepository repo,
        IProductRepository productRepo,
        IProductWarehouseStockRepository stockRepo,
        IUnitOfWork uow,
        ILogger<DeleteSalesReturnUseCase> logger)
    {
        _repo        = repo;
        _productRepo = productRepo;
        _stockRepo   = stockRepo;
        _uow         = uow;
        _logger      = logger;
    }

    public async Task ExecuteAsync(int id, CancellationToken ct = default)
    {
        var salesReturn = await _repo.GetByIdTrackedAsync(id, ct)
            ?? throw new DomainException($"Sales return with id {id} not found.");

        await _uow.BeginAsync(ct);
        try
        {
            // Không còn vòng đời Nháp: chứng từ luôn đã ghi sổ nên đã cộng tồn kho lúc lưu —
            // xóa phải rút lại. Hàng trả lại = nhập kho, nên hoàn tác = TRỪ tồn kho → kiểm tra
            // đủ tồn trước (đã xuất bán tiếp sau khi lưu thì không cho xóa), hai lượt như cũ.
            foreach (var line in salesReturn.Lines)
            {
                var product = await _productRepo.GetByIdTrackedAsync(line.ProductId, ct)
                    ?? throw new DomainException($"Product with id {line.ProductId} not found.");
                if (product.StockQuantity < line.Quantity)
                    throw new DomainException(
                        $"Không thể xóa vì tồn kho hiện tại của hàng hóa '{product.Name}' không đủ để " +
                        "hoàn tác số lượng đã nhập lại kho (đã phát sinh giao dịch xuất kho sau khi lưu).");
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

            await _repo.DeleteAsync(salesReturn, ct);
            await _uow.CommitAsync(ct);

            _logger.LogInformation("Deleted SalesReturn {Id} ({DocumentNumber})", id, salesReturn.DocumentNumber);
        }
        catch
        {
            await _uow.RollbackAsync(ct);
            throw;
        }
    }
}
