using Lamour.Application.Features.WarehouseReceipts.Dtos;

namespace Lamour.Application.Features.WarehouseReceipts.UseCases;

public interface IGetWarehouseReceiptByIdUseCase
{
    Task<WarehouseReceiptResponseDto?> ExecuteAsync(int id, CancellationToken ct = default);
}
