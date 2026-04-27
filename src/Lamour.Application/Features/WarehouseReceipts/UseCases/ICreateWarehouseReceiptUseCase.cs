using Lamour.Application.Features.WarehouseReceipts.Dtos;

namespace Lamour.Application.Features.WarehouseReceipts.UseCases;

public interface ICreateWarehouseReceiptUseCase
{
    Task<WarehouseReceiptResponseDto> ExecuteAsync(
        CreateWarehouseReceiptRequestDto request, CancellationToken ct = default);
}
