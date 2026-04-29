using Lamour.Application.Features.Accounting.Dtos;

namespace Lamour.Application.Features.Accounting.UseCases;

public interface IGetReceiptsUseCase
{
    Task<IEnumerable<ReceiptResponseDto>> ExecuteAsync(CancellationToken ct = default);
}
