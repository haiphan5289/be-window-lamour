using Lamour.Application.Features.Accounting.Repositories;
using Lamour.Domain.Entities;
using Lamour.Domain.Enums;
using Lamour.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace Lamour.Infrastructure.Repositories;

public class ReceiptRepository : IReceiptRepository
{
    private readonly AppDbContext _db;

    public ReceiptRepository(AppDbContext db) => _db = db;

    public async Task<IEnumerable<Receipt>> GetAllAsync(CancellationToken ct = default)
        => await _db.Receipts
            .AsNoTracking()
            .Include(r => r.Customer)
            .Include(r => r.CollectorEmployee)
            .Include(r => r.Entries).ThenInclude(e => e.DebitAccountSetting)
            .Include(r => r.Entries).ThenInclude(e => e.CreditAccountSetting)
            .OrderByDescending(r => r.CreatedAt)
            .ToListAsync(ct);

    public async Task<IEnumerable<Receipt>> GetUnconfirmedByDateRangeAsync(
        DateTime from, DateTime to, CancellationToken ct = default)
    {
        var utcFrom = DateTime.SpecifyKind(from, DateTimeKind.Utc);
        var utcTo   = DateTime.SpecifyKind(to,   DateTimeKind.Utc);
        return await _db.Receipts
            .AsNoTracking()
            .Where(r => r.Status != ReceiptStatus.Confirmed
                     && r.AccountingDate >= utcFrom
                     && r.AccountingDate <= utcTo)
            .Include(r => r.Entries).ThenInclude(e => e.DebitAccountSetting)
            .Include(r => r.Entries).ThenInclude(e => e.CreditAccountSetting)
            .OrderBy(r => r.AccountingDate)
            .ThenBy(r => r.Id)
            .ToListAsync(ct);
    }

    public async Task<Dictionary<string, int>> GetIdsByDocumentNumbersAsync(
        IEnumerable<string> documentNumbers, CancellationToken ct = default)
    {
        var numbers = documentNumbers.Where(n => !string.IsNullOrEmpty(n)).Distinct().ToList();
        if (numbers.Count == 0) return new Dictionary<string, int>();

        // Số chứng từ trùng (hiếm, do user tự nhập) → lấy phiếu tạo sau cùng.
        var rows = await _db.Receipts
            .AsNoTracking()
            .Where(x => numbers.Contains(x.DocumentNumber))
            .Select(x => new { x.DocumentNumber, x.Id })
            .ToListAsync(ct);
        return rows.GroupBy(r => r.DocumentNumber).ToDictionary(g => g.Key, g => g.Max(r => r.Id));
    }

    public async Task<Dictionary<string, Receipt>> GetByDocumentNumbersAsync(
        IEnumerable<string> documentNumbers, CancellationToken ct = default)
    {
        var numbers = documentNumbers.Where(n => !string.IsNullOrEmpty(n)).Distinct().ToList();
        if (numbers.Count == 0) return new Dictionary<string, Receipt>();

        var rows = await _db.Receipts
            .AsNoTracking()
            .Where(x => numbers.Contains(x.DocumentNumber))
            .Include(r => r.Entries).ThenInclude(e => e.DebitAccountSetting)
            .Include(r => r.Entries).ThenInclude(e => e.CreditAccountSetting)
            .ToListAsync(ct);
        return rows.GroupBy(r => r.DocumentNumber).ToDictionary(g => g.Key, g => g.OrderBy(r => r.Id).Last());
    }

    public async Task<Dictionary<int, string>> GetReasonDetailsByIdsAsync(
        IEnumerable<int> ids, CancellationToken ct = default)
    {
        var idList = ids.Distinct().ToList();
        if (idList.Count == 0) return new Dictionary<int, string>();

        var rows = await _db.Receipts
            .AsNoTracking()
            .Where(x => idList.Contains(x.Id) && x.ReasonDetail != null && x.ReasonDetail != "")
            .Select(x => new { x.Id, x.ReasonDetail })
            .ToListAsync(ct);
        return rows.ToDictionary(r => r.Id, r => r.ReasonDetail!);
    }

    public async Task<HashSet<int>> GetBulkReceiptIdsAsync(
        IEnumerable<int> receiptIds, CancellationToken ct = default)
    {
        var ids = receiptIds.Distinct().ToList();
        if (ids.Count == 0) return new HashSet<int>();

        var bulk = await _db.Receipts
            .AsNoTracking()
            .Where(x => ids.Contains(x.Id) && x.PartnerType == null)
            .Select(x => x.Id)
            .ToListAsync(ct);
        return bulk.ToHashSet();
    }

    public async Task<Receipt?> GetByIdAsync(int id, CancellationToken ct = default)
        => await _db.Receipts
            .AsNoTracking()
            .Include(r => r.Customer)
            .Include(r => r.CollectorEmployee)
            .Include(r => r.Entries).ThenInclude(e => e.DebitAccountSetting)
            .Include(r => r.Entries).ThenInclude(e => e.CreditAccountSetting)
            .FirstOrDefaultAsync(r => r.Id == id, ct);

    public async Task<Receipt?> GetByIdTrackedAsync(int id, CancellationToken ct = default)
        => await _db.Receipts
            .Include(r => r.Customer)
            .Include(r => r.CollectorEmployee)
            .Include(r => r.Entries).ThenInclude(e => e.DebitAccountSetting)
            .Include(r => r.Entries).ThenInclude(e => e.CreditAccountSetting)
            .FirstOrDefaultAsync(r => r.Id == id, ct);

    public async Task<Receipt> AddAsync(Receipt receipt, CancellationToken ct = default)
    {
        _db.Receipts.Add(receipt);
        await _db.SaveChangesAsync(ct);

        // Reload navigations
        await _db.Entry(receipt).Reference(r => r.Customer).LoadAsync(ct);
        if (receipt.CollectorEmployeeId.HasValue)
            await _db.Entry(receipt).Reference(r => r.CollectorEmployee).LoadAsync(ct);
        foreach (var entry in receipt.Entries)
        {
            await _db.Entry(entry).Reference(e => e.DebitAccountSetting).LoadAsync(ct);
            await _db.Entry(entry).Reference(e => e.CreditAccountSetting).LoadAsync(ct);
        }

        return receipt;
    }

    public async Task UpdateAsync(Receipt receipt, CancellationToken ct = default)
    {
        _db.Receipts.Update(receipt);
        await _db.SaveChangesAsync(ct);
    }

    public async Task DeleteAsync(Receipt receipt, CancellationToken ct = default)
    {
        _db.Receipts.Remove(receipt);
        await _db.SaveChangesAsync(ct);
    }

    public async Task<int> GetNextCodeNumberAsync(string prefix, CancellationToken ct = default)
    {
        var numbers = await _db.Receipts
            .AsNoTracking()
            .Select(r => r.DocumentNumber)
            .Where(n => n.StartsWith(prefix))
            .ToListAsync(ct);

        var max = numbers
            .Select(n => int.TryParse(n[prefix.Length..], out var num) ? num : 0)
            .DefaultIfEmpty(0)
            .Max();

        return max + 1;
    }

    public async Task<decimal> GetRemainingAmountAsync(int salesOrderId, CancellationToken ct = default)
    {
        var order = await _db.SalesOrders
            .AsNoTracking()
            .Where(o => o.Id == salesOrderId)
            .Select(o => (decimal?)o.GrandTotal)
            .FirstOrDefaultAsync(ct);
        if (order is null) return 0m;

        var paid = await _db.ReceiptEntries
            .AsNoTracking()
            .Where(e => e.SalesOrderId == salesOrderId)
            .SumAsync(e => (decimal?)e.Amount, ct) ?? 0m;

        var deducted = await _db.DepositDeductions
            .AsNoTracking()
            .Where(d => d.SalesOrderId == salesOrderId)
            .SumAsync(d => (decimal?)d.Amount, ct) ?? 0m;

        return order.Value - paid - deducted;
    }

    public async Task<IEnumerable<(
        int OrderId, string DocumentNumber, DateTime AccountingDate, DateTime DocumentDate,
        int CustomerId, string CustomerCode, string CustomerName, string? Description,
        decimal GrandTotal, string? PaymentTerms, DateTime? PaymentDueDate,
        decimal RemainingAmount)>> GetOutstandingSalesOrdersAsync(
        DateOnly fromDate, DateOnly toDate, int? employeeId, CancellationToken ct = default)
    {
        var fromUtc = DateTime.SpecifyKind(fromDate.ToDateTime(TimeOnly.MinValue), DateTimeKind.Utc);
        var toUtc   = DateTime.SpecifyKind(toDate.AddDays(1).ToDateTime(TimeOnly.MinValue), DateTimeKind.Utc);

        var rows = await _db.SalesOrders
            .AsNoTracking()
            .Where(o => o.Status == SalesOrderStatus.Normal
                     && o.AccountingDate >= fromUtc
                     && o.AccountingDate <  toUtc
                     && (!employeeId.HasValue || o.EmployeeId == employeeId.Value))
            .Select(o => new
            {
                o.Id,
                o.DocumentNumber,
                o.AccountingDate,
                o.DocumentDate,
                o.CustomerId,
                CustomerCode = o.Customer.Code,
                CustomerName = o.CustomerNameOverride ?? o.Customer.Name,
                o.Description,
                o.GrandTotal,
                o.PaymentTerms,
                o.PaymentDueDate,
                Paid     = _db.ReceiptEntries.Where(e => e.SalesOrderId == o.Id).Sum(e => (decimal?)e.Amount) ?? 0m,
                Deducted = _db.DepositDeductions.Where(d => d.SalesOrderId == o.Id).Sum(d => (decimal?)d.Amount) ?? 0m,
            })
            .ToListAsync(ct);

        return rows
            .Select(r => (
                r.Id, r.DocumentNumber, r.AccountingDate, r.DocumentDate,
                r.CustomerId, r.CustomerCode, r.CustomerName, r.Description,
                r.GrandTotal, r.PaymentTerms, r.PaymentDueDate,
                RemainingAmount: r.GrandTotal - r.Paid - r.Deducted))
            .Where(r => r.RemainingAmount > 0m)
            .OrderBy(r => r.AccountingDate).ThenBy(r => r.DocumentNumber);
    }

    public async Task<IEnumerable<(
        int OrderId, string DocumentNumber, DateTime AccountingDate, DateTime DocumentDate,
        int CustomerId, string CustomerCode, string CustomerName, string? Description,
        decimal GrandTotal, string? PaymentTerms, DateTime? PaymentDueDate,
        decimal RemainingAmount)>> GetSalesOrdersByIdsAsync(
        IEnumerable<int> salesOrderIds, CancellationToken ct = default)
    {
        var idList = salesOrderIds.Distinct().ToList();
        if (idList.Count == 0) return Enumerable.Empty<(int, string, DateTime, DateTime, int, string, string, string?, decimal, string?, DateTime?, decimal)>();

        var rows = await _db.SalesOrders
            .AsNoTracking()
            .Where(o => idList.Contains(o.Id))
            .Select(o => new
            {
                o.Id,
                o.DocumentNumber,
                o.AccountingDate,
                o.DocumentDate,
                o.CustomerId,
                CustomerCode = o.Customer.Code,
                CustomerName = o.CustomerNameOverride ?? o.Customer.Name,
                o.Description,
                o.GrandTotal,
                o.PaymentTerms,
                o.PaymentDueDate,
                Paid     = _db.ReceiptEntries.Where(e => e.SalesOrderId == o.Id).Sum(e => (decimal?)e.Amount) ?? 0m,
                Deducted = _db.DepositDeductions.Where(d => d.SalesOrderId == o.Id).Sum(d => (decimal?)d.Amount) ?? 0m,
            })
            .ToListAsync(ct);

        // Không lọc RemainingAmount > 0 / theo ngày — mục đích khác GetOutstandingSalesOrdersAsync
        // (xem doc comment ở interface): phải hiện lại đúng đơn đang gắn vào phiếu, kể cả đã hết nợ.
        return rows
            .Select(r => (
                r.Id, r.DocumentNumber, r.AccountingDate, r.DocumentDate,
                r.CustomerId, r.CustomerCode, r.CustomerName, r.Description,
                r.GrandTotal, r.PaymentTerms, r.PaymentDueDate,
                RemainingAmount: r.GrandTotal - r.Paid - r.Deducted));
    }
}
