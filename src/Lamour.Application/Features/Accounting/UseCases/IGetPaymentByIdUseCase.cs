using Lamour.Application.Features.Accounting.Dtos;

namespace Lamour.Application.Features.Accounting.UseCases;

public interface IGetPaymentByIdUseCase
{
    Task<PaymentResponseDto> ExecuteAsync(int id, CancellationToken ct = default);
}
