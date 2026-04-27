using Lamour.Application.Features.WarehouseReceipts.Dtos;

namespace Lamour.Application.Features.WarehouseReceipts.UseCases;

public interface IGetWarehouseReceiptsUseCase
{
    Task<IEnumerable<WarehouseReceiptResponseDto>> ExecuteAsync(CancellationToken ct = default);
}
