using Lamour.Application.Features.Warehouse.Dtos;

namespace Lamour.Application.Features.Warehouse.UseCases;

public interface IGetWarehouseTransactionsUseCase
{
    Task<IEnumerable<WarehouseTransactionResponseDto>> ExecuteAsync(
        DateTime? fromDate, DateTime? toDate, string? type, CancellationToken ct = default);
}
