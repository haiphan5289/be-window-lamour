using Lamour.Application.Features.Accounting.Dtos;

namespace Lamour.Application.Features.Accounting.UseCases;

public interface IUpdatePaymentUseCase
{
    Task<PaymentResponseDto> ExecuteAsync(int id, UpdatePaymentRequestDto request, CancellationToken ct = default);
}
