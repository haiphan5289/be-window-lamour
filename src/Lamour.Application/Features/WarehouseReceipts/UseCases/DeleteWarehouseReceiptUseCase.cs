using Lamour.Application.Features.WarehouseReceipts.Repositories;
using Lamour.Domain.Entities;
using Lamour.Domain.Exceptions;
using Microsoft.Extensions.Logging;

namespace Lamour.Application.Features.WarehouseReceipts.UseCases;

// Chỉ xóa được phiếu CHƯA ghi sổ (Draft) — phiếu đã ghi sổ đã cộng tồn kho nên phải "Bỏ ghi" trước
// (UnconfirmWarehouseReceiptUseCase hoàn tồn kho), cùng quy tắc xóa Chứng từ bán hàng.
public class DeleteWarehouseReceiptUseCase : IDeleteWarehouseReceiptUseCase
{
    private readonly IWarehouseReceiptRepository _repo;
    private readonly ILogger<DeleteWarehouseReceiptUseCase> _logger;

    public DeleteWarehouseReceiptUseCase(IWarehouseReceiptRepository repo, ILogger<DeleteWarehouseReceiptUseCase> logger)
    {
        _repo   = repo;
        _logger = logger;
    }

    public async Task ExecuteAsync(int id, CancellationToken ct = default)
    {
        var receipt = await _repo.GetByIdAsync(id, ct)
            ?? throw new NotFoundException($"WarehouseReceipt with id {id} not found.");

        if (receipt.Status == WarehouseReceiptStatus.Confirmed)
            throw new DomainException("Phiếu nhập kho đã ghi sổ — hãy Bỏ ghi trước khi xóa.");

        await _repo.DeleteAsync(receipt, ct);
        _logger.LogInformation("Deleted WarehouseReceipt {ReceiptNumber}", receipt.ReceiptNumber);
    }
}
