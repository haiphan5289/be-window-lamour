using Lamour.Application.Features.Warehouse.Repositories;
using Lamour.Application.Features.WarehouseReceipts.Dtos;
using Lamour.Application.Features.WarehouseReceipts.Repositories;
using Lamour.Domain.Entities;
using Lamour.Domain.Exceptions;
using Microsoft.Extensions.Logging;

namespace Lamour.Application.Features.WarehouseReceipts.UseCases;

public class UnconfirmWarehouseReceiptUseCase : IUnconfirmWarehouseReceiptUseCase
{
    private readonly IWarehouseReceiptRepository _repo;
    private readonly IProductWarehouseStockRepository _stockRepo;
    private readonly ILogger<UnconfirmWarehouseReceiptUseCase> _logger;

    public UnconfirmWarehouseReceiptUseCase(
        IWarehouseReceiptRepository repo,
        IProductWarehouseStockRepository stockRepo,
        ILogger<UnconfirmWarehouseReceiptUseCase> logger)
    {
        _repo      = repo;
        _stockRepo = stockRepo;
        _logger    = logger;
    }

    public async Task<WarehouseReceiptResponseDto> ExecuteAsync(int id, CancellationToken ct = default)
    {
        var receipt = await _repo.GetByIdAsync(id, ct)
            ?? throw new DomainException($"WarehouseReceipt with id {id} not found.");

        if (receipt.Status != WarehouseReceiptStatus.Confirmed)
            throw new DomainException("Only Confirmed receipts can be unconfirmed.");

        // Validate ALL lines first (two-pass) so we never partially revert stock if a later
        // line would fail — e.g. some of that stock has already been exported since confirming.
        foreach (var line in receipt.Lines)
        {
            if (line.Product is null)
                throw new DomainException($"Product with id {line.ProductId} not found.");

            var currentQty = await _stockRepo.GetQuantityAsync(line.ProductId, line.WarehouseId, ct);
            if (currentQty < line.Quantity)
                throw new DomainException(
                    $"Không thể bỏ ghi phiếu vì tồn kho hiện tại của hàng hóa '{line.Product.Name}' " +
                    "tại kho không đủ để hoàn tác (đã phát sinh giao dịch xuất kho sau khi ghi sổ).");
        }

        foreach (var line in receipt.Lines)
        {
            line.Product.StockQuantity -= line.Quantity;
            await _stockRepo.AdjustQuantityAsync(line.ProductId, line.WarehouseId, -line.Quantity, ct);
        }

        receipt.Status      = WarehouseReceiptStatus.Draft;
        receipt.ConfirmedAt = null;

        await _repo.SaveChangesAsync(ct);

        _logger.LogInformation(
            "Unconfirmed WarehouseReceipt {ReceiptNumber} — stock reverted for {LineCount} lines",
            receipt.ReceiptNumber, receipt.Lines.Count);

        return CreateWarehouseReceiptUseCase.MapToDto(receipt);
    }
}
