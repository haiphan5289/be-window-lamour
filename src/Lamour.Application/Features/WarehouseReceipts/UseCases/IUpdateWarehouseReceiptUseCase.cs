using Lamour.Application.Features.WarehouseReceipts.Dtos;

namespace Lamour.Application.Features.WarehouseReceipts.UseCases;

public interface IUpdateWarehouseReceiptUseCase
{
    Task<WarehouseReceiptResponseDto> ExecuteAsync(
        int id, UpdateWarehouseReceiptRequestDto request, CancellationToken ct = default);
}
