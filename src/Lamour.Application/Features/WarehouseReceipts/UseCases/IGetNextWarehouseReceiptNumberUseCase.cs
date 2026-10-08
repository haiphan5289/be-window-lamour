namespace Lamour.Application.Features.WarehouseReceipts.UseCases;

public interface IGetNextWarehouseReceiptNumberUseCase
{
    Task<string> ExecuteAsync(CancellationToken ct = default);
}
