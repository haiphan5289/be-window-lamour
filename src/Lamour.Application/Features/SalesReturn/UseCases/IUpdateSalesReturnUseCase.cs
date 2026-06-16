using Lamour.Application.Features.SalesReturn.Dtos;

namespace Lamour.Application.Features.SalesReturn.UseCases;

public interface IUpdateSalesReturnUseCase
{
    Task<SalesReturnResponseDto> ExecuteAsync(int id, UpdateSalesReturnRequestDto request, CancellationToken ct = default);
}
