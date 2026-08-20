using Lamour.Application.Features.Sales.Dtos;

namespace Lamour.Application.Features.Sales.UseCases;

public interface IGetSalesOrdersUseCase
{
    Task<IEnumerable<SalesOrderResponseDto>> ExecuteAsync(
        DateTime? fromDate = null, DateTime? toDate = null, string? search = null, CancellationToken ct = default);
}
