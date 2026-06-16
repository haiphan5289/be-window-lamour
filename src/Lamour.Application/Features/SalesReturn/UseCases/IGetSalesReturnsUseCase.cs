using Lamour.Application.Features.SalesReturn.Dtos;

namespace Lamour.Application.Features.SalesReturn.UseCases;

public interface IGetSalesReturnsUseCase
{
    Task<IEnumerable<SalesReturnResponseDto>> ExecuteAsync(CancellationToken ct = default);
}
