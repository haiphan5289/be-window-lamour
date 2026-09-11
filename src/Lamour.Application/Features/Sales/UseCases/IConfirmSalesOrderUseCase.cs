using Lamour.Application.Features.Sales.Dtos;

namespace Lamour.Application.Features.Sales.UseCases;

public interface IConfirmSalesOrderUseCase
{
    Task<SalesOrderResponseDto> ExecuteAsync(int id, CancellationToken ct = default);
}
