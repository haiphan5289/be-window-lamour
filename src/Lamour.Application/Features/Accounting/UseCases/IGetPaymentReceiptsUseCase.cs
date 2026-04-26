using Lamour.Application.Features.Accounting.Dtos;

namespace Lamour.Application.Features.Accounting.UseCases;

public interface IGetPaymentReceiptsUseCase
{
    Task<IEnumerable<PaymentReceiptResponseDto>> ExecuteAsync(CancellationToken ct = default);
}
