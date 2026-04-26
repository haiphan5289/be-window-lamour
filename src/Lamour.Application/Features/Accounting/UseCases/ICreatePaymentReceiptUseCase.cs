using Lamour.Application.Features.Accounting.Dtos;

namespace Lamour.Application.Features.Accounting.UseCases;

public interface ICreatePaymentReceiptUseCase
{
    Task<PaymentReceiptResponseDto> ExecuteAsync(CreatePaymentReceiptRequestDto request, CancellationToken ct = default);
}
