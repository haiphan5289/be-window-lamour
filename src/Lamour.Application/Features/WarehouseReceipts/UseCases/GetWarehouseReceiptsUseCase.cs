using Lamour.Application.Features.WarehouseReceipts.Dtos;
using Lamour.Application.Features.WarehouseReceipts.Repositories;

namespace Lamour.Application.Features.WarehouseReceipts.UseCases;

public class GetWarehouseReceiptsUseCase : IGetWarehouseReceiptsUseCase
{
    private readonly IWarehouseReceiptRepository _repo;

    public GetWarehouseReceiptsUseCase(IWarehouseReceiptRepository repo) => _repo = repo;

    public async Task<IEnumerable<WarehouseReceiptResponseDto>> ExecuteAsync(CancellationToken ct = default)
    {
        var receipts = await _repo.GetAllAsync(ct);
        return receipts.Select(CreateWarehouseReceiptUseCase.MapToDto).ToList();
    }
}
