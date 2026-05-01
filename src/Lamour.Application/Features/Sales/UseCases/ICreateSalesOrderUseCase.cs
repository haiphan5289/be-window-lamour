using Lamour.Application.Features.Sales.Dtos;

namespace Lamour.Application.Features.Sales.UseCases;

public interface ICreateSalesOrderUseCase
{
    Task<SalesOrderResponseDto> ExecuteAsync(CreateSalesOrderRequestDto request, CancellationToken ct = default);
}
