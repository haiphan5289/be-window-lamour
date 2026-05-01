using Lamour.Application.Features.Sales.Dtos;

namespace Lamour.Application.Features.Sales.UseCases;

public interface IGetSalesOrdersUseCase
{
    Task<IEnumerable<SalesOrderResponseDto>> ExecuteAsync(CancellationToken ct = default);
}
