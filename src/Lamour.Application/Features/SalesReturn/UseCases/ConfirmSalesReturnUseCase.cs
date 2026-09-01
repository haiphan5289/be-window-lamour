using Lamour.Application.Abstractions;
using Lamour.Application.Features.Products.Repositories;
using Lamour.Application.Features.SalesReturn.Dtos;
using Lamour.Application.Features.SalesReturn.Repositories;
using Lamour.Application.Features.Warehouse.Repositories;
using Lamour.Domain.Entities;
using Lamour.Domain.Exceptions;
using Microsoft.Extensions.Logging;

namespace Lamour.Application.Features.SalesReturn.UseCases;

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

        if (salesReturn.Status != SalesReturnStatus.Draft)
            throw new DomainException("Chỉ chứng từ ở trạng thái Nháp mới có thể ghi sổ.");

        await _uow.BeginAsync(ct);
        try
        {
            // Ghi sổ: cộng tồn kho cho từng dòng (hàng bán bị trả lại → nhập lại kho).
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

            _logger.LogInformation(
                "Confirmed SalesReturn {DocumentNumber} — {LineCount} lines, stock updated",
                salesReturn.DocumentNumber, salesReturn.Lines.Count);

            return GetSalesReturnsUseCase.MapToDto(salesReturn);
        }
        catch
        {
            await _uow.RollbackAsync(ct);
            throw;
        }
    }
}
