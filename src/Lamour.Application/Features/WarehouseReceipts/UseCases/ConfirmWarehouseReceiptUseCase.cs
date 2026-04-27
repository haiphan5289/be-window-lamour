using Lamour.Application.Features.WarehouseReceipts.Dtos;
using Lamour.Application.Features.WarehouseReceipts.Repositories;
using Lamour.Domain.Entities;
using Lamour.Domain.Exceptions;
using Microsoft.Extensions.Logging;

namespace Lamour.Application.Features.WarehouseReceipts.UseCases;

public class ConfirmWarehouseReceiptUseCase : IConfirmWarehouseReceiptUseCase
{
    private readonly IWarehouseReceiptRepository _repo;
    private readonly ILogger<ConfirmWarehouseReceiptUseCase> _logger;

    public ConfirmWarehouseReceiptUseCase(
        IWarehouseReceiptRepository repo,
        ILogger<ConfirmWarehouseReceiptUseCase> logger)
    {
        _repo   = repo;
        _logger = logger;
    }

    public async Task<WarehouseReceiptResponseDto> ExecuteAsync(int id, CancellationToken ct = default)
    {
        var receipt = await _repo.GetByIdAsync(id, ct)
            ?? throw new DomainException($"WarehouseReceipt with id {id} not found.");

        if (receipt.Status != WarehouseReceiptStatus.Draft)
            throw new DomainException("Only Draft receipts can be confirmed.");

        foreach (var line in receipt.Lines)
        {
            if (line.Product is null)
                throw new DomainException($"Product with id {line.ProductId} not found.");

            line.Product.StockQuantity += line.Quantity;
        }

        receipt.Status      = WarehouseReceiptStatus.Confirmed;
        receipt.ConfirmedAt = DateTime.UtcNow;

        await _repo.SaveChangesAsync(ct);

        _logger.LogInformation(
            "Confirmed WarehouseReceipt {ReceiptNumber} — {LineCount} lines, stock updated",
            receipt.ReceiptNumber, receipt.Lines.Count);

        return CreateWarehouseReceiptUseCase.MapToDto(receipt);
    }
}
