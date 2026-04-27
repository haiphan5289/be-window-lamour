using Lamour.Application.Features.WarehouseReceipts.Dtos;
using Lamour.Application.Features.WarehouseReceipts.Repositories;

namespace Lamour.Application.Features.WarehouseReceipts.UseCases;

public class GetWarehouseReceiptByIdUseCase : IGetWarehouseReceiptByIdUseCase
{
    private readonly IWarehouseReceiptRepository _repo;

    public GetWarehouseReceiptByIdUseCase(IWarehouseReceiptRepository repo) => _repo = repo;

    public async Task<WarehouseReceiptResponseDto?> ExecuteAsync(int id, CancellationToken ct = default)
    {
        var receipt = await _repo.GetByIdAsync(id, ct);
        return receipt is null ? null : CreateWarehouseReceiptUseCase.MapToDto(receipt);
    }
}
