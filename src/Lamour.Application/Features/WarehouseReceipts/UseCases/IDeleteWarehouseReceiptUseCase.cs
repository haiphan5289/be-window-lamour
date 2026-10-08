namespace Lamour.Application.Features.WarehouseReceipts.UseCases;

public interface IDeleteWarehouseReceiptUseCase
{
    Task ExecuteAsync(int id, CancellationToken ct = default);
}
