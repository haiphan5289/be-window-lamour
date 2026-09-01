using Lamour.Application.Features.Accounting.Dtos;

namespace Lamour.Application.Features.Accounting.UseCases;

public interface IUnconfirmReceiptUseCase
{
    Task<ReceiptResponseDto> ExecuteAsync(int id, CancellationToken ct = default);
}
