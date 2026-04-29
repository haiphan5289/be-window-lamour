using Lamour.Application.Features.Accounting.Dtos;

namespace Lamour.Application.Features.Accounting.UseCases;

public interface IGetPaymentsUseCase
{
    Task<IEnumerable<PaymentResponseDto>> ExecuteAsync(CancellationToken ct = default);
}
