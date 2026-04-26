using Lamour.Domain.Entities;

namespace Lamour.Application.Features.Accounting.Repositories;

public interface IPaymentReceiptRepository
{
    Task<IEnumerable<PaymentReceipt>> GetAllAsync(CancellationToken ct = default);
    Task<PaymentReceipt> AddAsync(PaymentReceipt receipt, CancellationToken ct = default);
    Task<string> GetNextReceiptNumberAsync(DateTime date, CancellationToken ct = default);
}
