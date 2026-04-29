using Lamour.Application.Features.Accounting.Dtos;

namespace Lamour.Application.Features.Accounting.UseCases;

public interface IDuplicatePaymentUseCase
{
    Task<PaymentResponseDto> ExecuteAsync(int id, CancellationToken ct = default);
}
