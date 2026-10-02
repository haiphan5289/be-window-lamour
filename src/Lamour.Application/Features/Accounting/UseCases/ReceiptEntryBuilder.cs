using Lamour.Application.Features.AccountSettings.Repositories;
using Lamour.Application.Features.Accounting.Dtos;
using Lamour.Application.Features.Accounting.Repositories;
using Lamour.Application.Features.Customers.Repositories;
using Lamour.Application.Features.Employees.Repositories;
using Lamour.Domain.Entities;
using Lamour.Domain.Enums;
using Lamour.Domain.Exceptions;

namespace Lamour.Application.Features.Accounting.UseCases;

/// <summary>
/// Phần validate dùng chung của Create/Update phiếu thu: Lý do nộp, Đối tượng (Khách hàng / Nhân viên)
/// và TK Nợ/Có của từng dòng hạch toán.
/// </summary>
internal static class ReceiptEntryBuilder
{
    // Tên TK kiểu cũ (enum AccountCode, còn dùng ở request Phiếu thu hàng loạt) → mã trong danh mục TK.
    private static readonly Dictionary<string, string> LegacyAccountCodes = new()
    {
        ["Cash111"]       = "1111",
        ["Bank112"]       = "1121",
        ["Receivable131"] = "131",
        ["Payroll334"]    = "334",
    };

    public static PaymentReason ParseReason(string value)
    {
        if (!Enum.TryParse<PaymentReason>(value, out var reason))
            throw new DomainException($"Invalid payment_reason '{value}'. Valid values: ThuKhac, RutTienGuiVeNopQuy, ThuHoanThueGTGT, ThuHoanUng (and legacy ThuTienHang, ThuCongNo).");
        return reason;
    }

    /// <summary>
    /// Đối tượng của phiếu thu. Trả <c>null</c> cho phiếu thu hàng loạt (không có đối tượng ở header).
    /// </summary>
    public static async Task<(PaymentPartnerType Type, int Id, string Name)?> ResolvePartnerAsync(
        string? partnerType, int? partnerId, PaymentReason reason,
        ICustomerRepository customerRepo, IEmployeeRepository employeeRepo, CancellationToken ct)
    {
        if (string.IsNullOrWhiteSpace(partnerType) && partnerId is null)
        {
            if (reason != PaymentReason.ThuKhachHangHangLoat)
                throw new DomainException("Vui lòng chọn đối tượng.");
            return null;
        }

        if (!Enum.TryParse<PaymentPartnerType>(partnerType, out var type) || partnerId is null)
            throw new DomainException($"Invalid partner_type '{partnerType}'. Valid values: Customer, Employee.");

        switch (type)
        {
            case PaymentPartnerType.Customer:
                var customer = await customerRepo.GetByIdAsync(partnerId.Value, ct)
                    ?? throw new DomainException("Khách hàng không tồn tại.");
                return (type, partnerId.Value, customer.Name);

            case PaymentPartnerType.Employee:
                var employee = await employeeRepo.GetByIdAsync(partnerId.Value, ct)
                    ?? throw new DomainException("Nhân viên không tồn tại.");
                return (type, partnerId.Value, employee.Name);

            default:
                throw new DomainException("Đối tượng của phiếu thu chỉ là Khách hàng hoặc Nhân viên.");
        }
    }

    public static void ApplyPartner(Receipt receipt, (PaymentPartnerType Type, int Id, string Name)? partner)
    {
        receipt.PartnerType = partner?.Type;
        receipt.PartnerId   = partner?.Id;
        receipt.PartnerName = partner?.Name;
        receipt.CustomerId  = partner?.Type == PaymentPartnerType.Customer ? partner.Value.Id : null;
    }

    public static async Task<List<ReceiptEntry>> BuildEntriesAsync(
        IEnumerable<ReceiptEntryDto> dtos, IReceiptRepository repo,
        IAccountSettingRepository accountRepo, bool validateRemaining, CancellationToken ct)
    {
        List<AccountSetting>? allAccounts = null;

        async Task<int> ResolveAsync(int id, string? legacyName, string label)
        {
            if (id > 0)
            {
                if (await accountRepo.GetByIdAsync(id, ct) is null)
                    throw new DomainException($"Tài khoản {label} không tồn tại.");
                return id;
            }

            if (string.IsNullOrWhiteSpace(legacyName))
                throw new DomainException($"Vui lòng chọn tài khoản {label}.");
            if (!LegacyAccountCodes.TryGetValue(legacyName, out var code))
                throw new DomainException($"Tài khoản {label} '{legacyName}' không hợp lệ.");

            allAccounts ??= (await accountRepo.GetAllAsync(ct)).ToList();
            var account = allAccounts.FirstOrDefault(a => string.Equals(a.Code, code, StringComparison.OrdinalIgnoreCase))
                ?? throw new DomainException($"Danh mục tài khoản chưa có TK {code}.");
            return account.Id;
        }

        var entries = new List<ReceiptEntry>();
        foreach (var e in dtos)
        {
            var debitId  = await ResolveAsync(e.DebitAccountId,  e.DebitAccount,  "Nợ");
            var creditId = await ResolveAsync(e.CreditAccountId, e.CreditAccount, "Có");

            // Dòng gắn với 1 Chứng từ bán hàng (Phiếu thu hàng loạt khách hàng) — chặn thu quá số
            // còn nợ thật (không tin remaining_amount client tính lúc search, giá trị có thể lệch
            // do có phiếu thu khác vừa tạo song song).
            if (validateRemaining && e.SalesOrderId.HasValue)
            {
                var remaining = await repo.GetRemainingAmountAsync(e.SalesOrderId.Value, ct);
                if (e.Amount > remaining)
                    throw new DomainException(
                        $"Số tiền thu ({e.Amount:N0}) vượt quá số còn nợ thực tế ({remaining:N0}) của đơn hàng.");
            }

            entries.Add(new ReceiptEntry
            {
                Description            = e.Description,
                DebitAccountSettingId  = debitId,
                CreditAccountSettingId = creditId,
                Amount                 = e.Amount,
                SubjectCode            = e.SubjectCode,
                SubjectName            = e.SubjectName,
                BankAccount            = e.BankAccount,
                SalesOrderId           = e.SalesOrderId,
            });
        }
        return entries;
    }

    // Sổ quỹ ghi theo TK cấp 1: 1111 → "111", 1121 → "112"; TK khác giữ nguyên mã.
    public static string LedgerAccount(string? code) => code switch
    {
        null or ""                          => "111",
        _ when code.StartsWith("111")       => "111",
        _ when code.StartsWith("112")       => "112",
        _                                   => code,
    };

    public static string DocumentType(Receipt receipt) =>
        receipt.IsBulk                                      ? "Phiếu thu tiền mặt khách hàng hàng loạt"
        : receipt.PartnerType == PaymentPartnerType.Customer ? "Phiếu thu tiền mặt khách hàng"
        :                                                     "Phiếu thu";
}
