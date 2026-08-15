using Lamour.Application.Features.Deposits.Repositories;
using Lamour.Domain.Entities;
using Lamour.Domain.Exceptions;

namespace Lamour.Application.Features.Sales.UseCases;

// Cầu nối Sales Order ↔ Deposit: khi 1 Sales Order có dòng dùng sản phẩm "Đặt cọc"
// (Product.IsDepositProduct = true), tự động tạo/đồng bộ/xóa 1 Deposit ngầm gắn với đơn hàng đó
// (Deposit.SourceSalesOrderId), để màn "Trừ cọc" có thể tìm/trừ theo đúng số chứng từ BC gốc.
internal static class SalesOrderDepositHelper
{
    // Gọi SAU KHI SalesOrder đã có Id (đã AddAsync/đã tồn tại khi Update).
    public static async Task SyncAsync(
        IDepositRepository depositRepo, SalesOrder order, decimal depositLinesAmount, CancellationToken ct)
    {
        var existing = await depositRepo.GetBySourceSalesOrderIdAsync(order.Id, ct);

        if (depositLinesAmount <= 0)
        {
            if (existing is null) return;
            if (existing.RemainingBalance != existing.Amount)
                throw new DomainException("Cọc từ đơn hàng này đã bị trừ, không thể xóa dòng Đặt cọc.");
            await depositRepo.DeleteAsync(existing, ct);
            return;
        }

        if (existing is null)
        {
            var nextNum = await depositRepo.GetNextCodeNumberAsync(ct);
            var deposit = new Deposit
            {
                DocumentNumber     = $"DC{nextNum:D5}",
                AccountingDate     = order.AccountingDate,
                DocumentDate       = order.DocumentDate,
                CustomerId         = order.CustomerId,
                EmployeeId         = order.EmployeeId,
                Description        = $"Đặt cọc từ đơn {order.DocumentNumber}",
                Amount             = depositLinesAmount,
                RemainingBalance   = depositLinesAmount,
                Status             = DepositStatus.Active,
                CreatedAt          = DateTime.UtcNow,
                SourceSalesOrderId = order.Id,
            };
            await depositRepo.AddAsync(deposit, ct);
            return;
        }

        if (existing.Amount != depositLinesAmount && existing.RemainingBalance != existing.Amount)
            throw new DomainException("Cọc từ đơn hàng này đã bị trừ, không thể đổi số tiền Đặt cọc.");

        if (existing.Amount != depositLinesAmount)
        {
            existing.Amount           = depositLinesAmount;
            existing.RemainingBalance = depositLinesAmount;
            existing.Status           = DepositStatus.Active;
        }
        existing.CustomerId     = order.CustomerId;
        existing.EmployeeId     = order.EmployeeId;
        existing.AccountingDate = order.AccountingDate;
        existing.DocumentDate   = order.DocumentDate;
        await depositRepo.UpdateAsync(existing, ct);
    }

    // Gọi TRƯỚC KHI xóa SalesOrder — chặn nếu cọc đã bị trừ, tự xóa cọc nếu chưa đụng tới.
    public static async Task GuardAndDeleteLinkedDepositAsync(
        IDepositRepository depositRepo, int salesOrderId, CancellationToken ct)
    {
        var existing = await depositRepo.GetBySourceSalesOrderIdAsync(salesOrderId, ct);
        if (existing is null) return;

        if (existing.RemainingBalance != existing.Amount)
            throw new DomainException("Đơn hàng này đã tạo cọc và cọc đã bị trừ, không thể xóa.");

        await depositRepo.DeleteAsync(existing, ct);
    }
}
