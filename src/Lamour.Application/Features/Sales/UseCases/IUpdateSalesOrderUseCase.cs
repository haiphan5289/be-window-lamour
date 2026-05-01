using Lamour.Application.Features.Sales.Dtos;

namespace Lamour.Application.Features.Sales.UseCases;

public interface IUpdateSalesOrderUseCase
{
    Task<SalesOrderResponseDto> ExecuteAsync(int id, UpdateSalesOrderRequestDto request, CancellationToken ct = default);
}
