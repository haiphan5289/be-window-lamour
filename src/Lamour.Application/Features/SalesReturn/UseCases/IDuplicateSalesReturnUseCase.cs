using Lamour.Application.Features.SalesReturn.Dtos;

namespace Lamour.Application.Features.SalesReturn.UseCases;

public interface IDuplicateSalesReturnUseCase
{
    Task<SalesReturnResponseDto> ExecuteAsync(int id, CancellationToken ct = default);
}
