using Lamour.Application.Features.Accounting.Dtos;
using Lamour.Application.Features.Accounting.Repositories;
using Lamour.Domain.Entities;
using Lamour.Domain.Enums;
using Lamour.Domain.Exceptions;
using Microsoft.Extensions.Logging;

namespace Lamour.Application.Features.Accounting.UseCases;

public class CreateReceiptUseCase : ICreateReceiptUseCase
{
    private readonly IReceiptRepository     _repo;
    private readonly ILogger<CreateReceiptUseCase> _logger;

    public CreateReceiptUseCase(
        IReceiptRepository repo,
        ILogger<CreateReceiptUseCase> logger)
    {
        _repo     = repo;
        _logger   = logger;
    }

    public async Task<ReceiptResponseDto> ExecuteAsync(
        CreateReceiptRequestDto request, CancellationToken ct = default)
    {
        if (!Enum.TryParse<PaymentReason>(request.PaymentReason, out var paymentReason))
            throw new DomainException($"Invalid payment_reason '{request.PaymentReason}'. Valid values: ThuKhac, ThuTienHang, ThuCongNo.");

        var entries = new List<ReceiptEntry>();
        foreach (var e in request.Entries)
        {
            if (!Enum.TryParse<AccountCode>(e.DebitAccount, out var debit))
                throw new DomainException($"Invalid debit_account '{e.DebitAccount}'.");
            if (!Enum.TryParse<AccountCode>(e.CreditAccount, out var credit))
                throw new DomainException($"Invalid credit_account '{e.CreditAccount}'.");

            // Dòng gắn với 1 Chứng từ bán hàng (Phiếu thu hàng loạt khách hàng) — chặn thu quá số
            // còn nợ thật (không tin remaining_amount client tính lúc search, giá trị có thể lệch
            // do có phiếu thu khác vừa tạo song song).
            if (e.SalesOrderId.HasValue)
            {
                var remaining = await _repo.GetRemainingAmountAsync(e.SalesOrderId.Value, ct);
                if (e.Amount > remaining)
                    throw new DomainException(
                        $"Số tiền thu ({e.Amount:N0}) vượt quá số còn nợ thực tế ({remaining:N0}) của đơn hàng.");
            }

            entries.Add(new ReceiptEntry
            {
                Description   = e.Description,
                DebitAccount  = debit,
                CreditAccount = credit,
                Amount        = e.Amount,
                SubjectCode   = e.SubjectCode,
                SubjectName   = e.SubjectName,
                BankAccount   = e.BankAccount,
                SalesOrderId  = e.SalesOrderId,
            });
        }

        var receipt = new Receipt
        {
            CustomerId          = request.CustomerId,
            PayerName           = request.PayerName,
            Address             = request.Address,
            PaymentReason       = paymentReason,
            CollectorEmployeeId = request.CollectorEmployeeId,
            Attachment          = request.Attachment,
            Reference           = request.Reference,
            AccountingDate      = DateTime.SpecifyKind(request.AccountingDate, DateTimeKind.Utc),
            DocumentDate        = DateTime.SpecifyKind(request.DocumentDate, DateTimeKind.Utc),
            DocumentNumber      = request.DocumentNumber,
            CreatedAt           = DateTime.UtcNow,
            Entries             = entries,
        };

        var saved = await _repo.AddAsync(receipt, ct);

        // Draft receipt has no cash-ledger effect yet — CashTransaction is now posted only on
        // Confirm ("Ghi sổ"), mirroring Payment/SalesReturn. See ConfirmReceiptUseCase.

        _logger.LogInformation("Created Receipt {DocumentNumber} for customer {CustomerId}",
            saved.DocumentNumber, saved.CustomerId);

        return GetReceiptsUseCase.MapToDto(saved);
    }

    internal static string MapAccountCodeToString(AccountCode code) => code switch
    {
        AccountCode.Cash111       => "111",
        AccountCode.Bank112       => "112",
        AccountCode.Receivable131 => "131",
        AccountCode.Payroll334    => "334",
        _                         => "131",
    };
}
