using Lamour.Application.Features.Accounting.Dtos;

namespace Lamour.Application.Features.Accounting.UseCases;

public interface ICreatePaymentUseCase
{
    Task<PaymentResponseDto> ExecuteAsync(CreatePaymentRequestDto request, CancellationToken ct = default);
}
