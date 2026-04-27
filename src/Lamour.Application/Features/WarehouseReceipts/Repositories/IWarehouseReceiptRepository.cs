using Lamour.Domain.Entities;

namespace Lamour.Application.Features.WarehouseReceipts.Repositories;

public interface IWarehouseReceiptRepository
{
    Task<IEnumerable<WarehouseReceipt>> GetAllAsync(CancellationToken ct = default);
    Task<WarehouseReceipt?> GetByIdAsync(int id, CancellationToken ct = default);
    Task<WarehouseReceipt> AddAsync(WarehouseReceipt receipt, CancellationToken ct = default);
    Task SaveChangesAsync(CancellationToken ct = default);
    Task<string> GetNextReceiptNumberAsync(DateTime date, CancellationToken ct = default);
}
