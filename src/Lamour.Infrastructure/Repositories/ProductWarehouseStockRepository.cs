using Lamour.Application.Features.Warehouse.Repositories;
using Lamour.Domain.Entities;
using Lamour.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace Lamour.Infrastructure.Repositories;

public class ProductWarehouseStockRepository : IProductWarehouseStockRepository
{
    private readonly AppDbContext _db;

    public ProductWarehouseStockRepository(AppDbContext db) => _db = db;

    public async Task<int> GetQuantityAsync(int productId, int warehouseId, CancellationToken ct = default)
    {
        var stock = await _db.ProductWarehouseStocks.AsNoTracking()
            .FirstOrDefaultAsync(x => x.ProductId == productId && x.WarehouseId == warehouseId, ct);
        return stock?.Quantity ?? 0;
    }

    public async Task AdjustQuantityAsync(int productId, int warehouseId, int delta, CancellationToken ct = default)
    {
        var stock = await _db.ProductWarehouseStocks
            .FirstOrDefaultAsync(x => x.ProductId == productId && x.WarehouseId == warehouseId, ct);

        if (stock is null)
        {
            stock = new ProductWarehouseStock { ProductId = productId, WarehouseId = warehouseId, Quantity = 0 };
            _db.ProductWarehouseStocks.Add(stock);
        }

        stock.Quantity += delta;
        await _db.SaveChangesAsync(ct);
    }

    public async Task<int> GetTotalQuantityAsync(int productId, CancellationToken ct = default)
        => await _db.ProductWarehouseStocks.AsNoTracking()
            .Where(x => x.ProductId == productId)
            .SumAsync(x => x.Quantity, ct);
}
