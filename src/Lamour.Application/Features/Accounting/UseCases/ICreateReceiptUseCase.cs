using Lamour.Application.Features.Accounting.Dtos;

namespace Lamour.Application.Features.Accounting.UseCases;

public interface ICreateReceiptUseCase
{
    Task<ReceiptResponseDto> ExecuteAsync(CreateReceiptRequestDto request, CancellationToken ct = default);
}
