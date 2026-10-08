using Lamour.Application.Features.Products.Repositories;
using Lamour.Domain.Entities;
using Lamour.Domain.Exceptions;
using Lamour.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;
using Npgsql;

namespace Lamour.Infrastructure.Repositories;

public class ProductRepository : IProductRepository
{
    private readonly AppDbContext _db;

    public ProductRepository(AppDbContext db) => _db = db;

    private static IQueryable<Product> IncludeAll(IQueryable<Product> query) => query
        .Include(p => p.Category)
        .Include(p => p.ProductUnit)
        .Include(p => p.DefaultWarehouse)
        .Include(p => p.StockAccount)
        .Include(p => p.RevenueAccount)
        .Include(p => p.DiscountAccount)
        .Include(p => p.PriceReductionAccount)
        .Include(p => p.ReturnAccount)
        .Include(p => p.CostAccount);

    public async Task<IEnumerable<Product>> GetAllAsync(CancellationToken ct = default)
        => await IncludeAll(_db.Products.AsNoTracking()).ToListAsync(ct);

    public async Task<Product?> GetByIdAsync(int id, CancellationToken ct = default)
        => await IncludeAll(_db.Products.AsNoTracking()).FirstOrDefaultAsync(p => p.Id == id, ct);

    public async Task<Product?> GetByIdTrackedAsync(int id, CancellationToken ct = default)
        => await _db.Products.FirstOrDefaultAsync(p => p.Id == id, ct);

    public async Task<bool> CodeExistsAsync(string code, int? excludeId = null, CancellationToken ct = default)
        => await _db.Products.AsNoTracking()
            .AnyAsync(p => p.Code.ToLower() == code.ToLower() && (excludeId == null || p.Id != excludeId), ct);

    // Mã tự tăng nối tiếp dãy số của MISA (1, 2, ... 69 → 70): chỉ xét mã toàn chữ số, bỏ qua mã chữ
    // (CPMH, LPXD, DATCOC, SP001...).
    public async Task<string> GetNextCodeAsync(CancellationToken ct = default)
    {
        var codes = await _db.Products.AsNoTracking().Select(p => p.Code).ToListAsync(ct);
        var maxNum = codes
            .Where(c => c.Length > 0 && c.All(char.IsAsciiDigit) && long.TryParse(c, out _))
            .Select(long.Parse)
            .DefaultIfEmpty(0)
            .Max();
        return (maxNum + 1).ToString();
    }

    public async Task<Product> AddAsync(Product product, CancellationToken ct = default)
    {
        _db.Products.Add(product);
        await _db.SaveChangesAsync(ct);
        return product;
    }

    public async Task AddRangeAsync(IEnumerable<Product> products, CancellationToken ct = default)
    {
        _db.Products.AddRange(products);
        await _db.SaveChangesAsync(ct);
    }

    public async Task<Product> UpdateAsync(Product product, CancellationToken ct = default)
    {
        // Chỉ mark root entity Modified — KHÔNG dùng _db.Products.Update(product), vì product được load
        // qua GetByIdAsync (AsNoTracking + Include Category/ProductUnit/DefaultWarehouse/6 AccountSetting).
        // DbSet.Update() cascade toàn bộ graph reachable thành Modified, ghi đè nhầm các bảng lookup dùng chung
        // bằng giá trị đã load trước đó (lost-update nếu bảng đó vừa bị sửa bởi request khác).
        _db.Entry(product).State = EntityState.Modified;
        await _db.SaveChangesAsync(ct);
        return product;
    }

    public async Task DeleteAsync(Product product, CancellationToken ct = default)
    {
        _db.Products.Remove(product);
        try
        {
            await _db.SaveChangesAsync(ct);
        }
        catch (DbUpdateException ex) when (ex.InnerException is PostgresException { SqlState: PostgresErrorCodes.ForeignKeyViolation })
        {
            throw new DomainException($"Không thể xóa sản phẩm '{product.Name}' vì đang có trong đơn bán hàng, phiếu nhập kho hoặc hàng bán bị trả lại.");
        }
    }
}
