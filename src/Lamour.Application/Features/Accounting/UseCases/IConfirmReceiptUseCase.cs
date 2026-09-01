using Lamour.Application.Features.Accounting.Dtos;

namespace Lamour.Application.Features.Accounting.UseCases;

public interface IConfirmReceiptUseCase
{
    Task<ReceiptResponseDto> ExecuteAsync(int id, CancellationToken ct = default);
}
