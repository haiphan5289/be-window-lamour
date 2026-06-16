using Lamour.Application.Features.SalesReturn.Dtos;

namespace Lamour.Application.Features.SalesReturn.UseCases;

public interface ICreateSalesReturnUseCase
{
    Task<SalesReturnResponseDto> ExecuteAsync(CreateSalesReturnRequestDto request, CancellationToken ct = default);
}
