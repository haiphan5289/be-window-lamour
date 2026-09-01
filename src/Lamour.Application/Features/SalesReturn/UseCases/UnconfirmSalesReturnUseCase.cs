using Lamour.Application.Abstractions;
using Lamour.Application.Features.Products.Repositories;
using Lamour.Application.Features.SalesReturn.Dtos;
using Lamour.Application.Features.SalesReturn.Repositories;
using Lamour.Application.Features.Warehouse.Repositories;
using Lamour.Domain.Entities;
using Lamour.Domain.Exceptions;
using Microsoft.Extensions.Logging;

namespace Lamour.Application.Features.SalesReturn.UseCases;

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
            throw new DomainException("Chỉ chứng từ đã ghi sổ mới có thể bỏ ghi.");

        await _uow.BeginAsync(ct);
        try
        {
            // Validate ALL lines first (two-pass) so we never partially revert stock if a later
            // line would fail — e.g. some of that stock has already been exported since confirming.
            foreach (var line in salesReturn.Lines)
            {
                var product = await _productRepo.GetByIdTrackedAsync(line.ProductId, ct)
                    ?? throw new DomainException($"Product with id {line.ProductId} not found.");

                if (product.StockQuantity < line.Quantity)
                    throw new DomainException(
                        $"Không thể bỏ ghi vì tồn kho hiện tại của hàng hóa '{product.Name}' không đủ " +
                        "để hoàn tác (đã phát sinh giao dịch xuất kho sau khi ghi sổ).");
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

            salesReturn.Status      = SalesReturnStatus.Draft;
            salesReturn.ConfirmedAt = null;

            await _repo.UpdateAsync(salesReturn, ct);
            await _uow.CommitAsync(ct);

            _logger.LogInformation(
                "Unconfirmed SalesReturn {DocumentNumber} — stock reverted for {LineCount} lines",
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
