namespace Lamour.Application.Features.Warehouse.Repositories;

public interface IProductWarehouseStockRepository
{
    Task<int> GetQuantityAsync(int productId, int warehouseId, CancellationToken ct = default);

    // Get-or-create row cho (productId, warehouseId), cộng/trừ delta, tự SaveChanges.
    Task AdjustQuantityAsync(int productId, int warehouseId, int delta, CancellationToken ct = default);

    // Tổng tồn kho hiện tại của product trên tất cả kho — dùng để đồng bộ lại Product.StockQuantity (cache).
    Task<int> GetTotalQuantityAsync(int productId, CancellationToken ct = default);
}
