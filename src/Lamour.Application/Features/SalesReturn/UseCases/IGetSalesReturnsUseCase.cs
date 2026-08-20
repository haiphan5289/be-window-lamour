using Lamour.Application.Features.SalesReturn.Dtos;

namespace Lamour.Application.Features.SalesReturn.UseCases;

public interface IGetSalesReturnsUseCase
{
    Task<IEnumerable<SalesReturnResponseDto>> ExecuteAsync(
        DateTime? fromDate = null, DateTime? toDate = null, string? search = null, CancellationToken ct = default);
}
