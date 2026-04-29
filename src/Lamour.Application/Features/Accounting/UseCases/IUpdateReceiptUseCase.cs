using Lamour.Application.Features.Accounting.Dtos;

namespace Lamour.Application.Features.Accounting.UseCases;

public interface IUpdateReceiptUseCase
{
    Task<ReceiptResponseDto> ExecuteAsync(int id, UpdateReceiptRequestDto request, CancellationToken ct = default);
}
