using Lamour.Domain.Entities;

namespace Lamour.Application.Features.Warehouse.Repositories;

public interface IInventoryRepository
{
    Task<IEnumerable<Product>> GetAllActiveAsync(CancellationToken ct = default);

    // Returns confirmed import totals per product for the given date range.
    // Key = ProductId, Value = (total quantity, total amount).
    Task<Dictionary<int, (int Qty, decimal Value)>> GetImportsByProductAsync(
        DateOnly fromDate, DateOnly toDate, CancellationToken ct = default);
}
