using Lamour.Application.Features.Accounting.Dtos;

namespace Lamour.Application.Features.Accounting.UseCases;

public interface IGetSalesOrdersByIdsUseCase
{
    Task<IEnumerable<OutstandingSalesOrderDto>> ExecuteAsync(
        IEnumerable<int> salesOrderIds, CancellationToken ct = default);
}
