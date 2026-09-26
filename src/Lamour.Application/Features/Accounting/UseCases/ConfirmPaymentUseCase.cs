using Lamour.Application.Features.Accounting.Dtos;
using Lamour.Application.Features.Accounting.Repositories;
using Lamour.Domain.Entities;
using Lamour.Domain.Exceptions;
using Microsoft.Extensions.Logging;

namespace Lamour.Application.Features.Accounting.UseCases;

public class ConfirmPaymentUseCase : IConfirmPaymentUseCase
{
    private readonly IPaymentRepository    _repo;
    private readonly ICashLedgerRepository _cashRepo;
    private readonly ILogger<ConfirmPaymentUseCase> _logger;

    public ConfirmPaymentUseCase(
        IPaymentRepository repo,
        ICashLedgerRepository cashRepo,
        ILogger<ConfirmPaymentUseCase> logger)
    {
        _repo     = repo;
        _cashRepo = cashRepo;
        _logger   = logger;
    }

    public async Task<PaymentResponseDto> ExecuteAsync(int id, CancellationToken ct = default)
    {
        var payment = await _repo.GetByIdTrackedAsync(id, ct)
            ?? throw new NotFoundException($"Payment with id {id} not found.");

        // 2026-09-26: bỏ khái niệm "Nháp" khỏi quy trình (khớp Chứng từ bán hàng — chỉ còn Treo / Đã
        // ghi sổ). Draft cũ được coi như Treo nên ghi sổ thẳng được — trước đây bắt buộc Treo khiến
        // nút "Cất" (= Ghi sổ) trên phiếu chi mới tạo (Draft) luôn bị từ chối.
        if (payment.Status == PaymentStatus.Confirmed)
            throw new DomainException("Phiếu chi này đã được ghi sổ.");

        if (payment.Entries.Count == 0)
            throw new DomainException("Phiếu chi phải có ít nhất 1 dòng hạch toán.");

        var totalAmount     = payment.Entries.Sum(e => e.Amount);
        var counterAccount  = payment.Entries.First().DebitAccountSetting.Code;

        await _cashRepo.AddAsync(new CashTransaction
        {
            AccountingDate = payment.AccountingDate,
            DocumentDate   = payment.DocumentDate,
            ReceiptNumber  = null,
            PaymentNumber  = payment.DocumentNumber,
            Description    = payment.PayeeName,
            Account        = "111",
            CounterAccount = counterAccount,
            DebitAmount    = 0m,
            CreditAmount   = totalAmount,  // Payment decreases cash
            PersonName     = payment.PayeeName,
            PaymentReason  = payment.PaymentReason.ToString(),
            DocumentType   = "Phiếu chi",
            CreatedAt      = DateTime.UtcNow,
        }, ct);

        payment.Status      = PaymentStatus.Confirmed;
        payment.ConfirmedAt = DateTime.UtcNow;
        await _repo.UpdateAsync(payment, ct);

        _logger.LogInformation("Confirmed Payment {Id} ({DocumentNumber})", id, payment.DocumentNumber);

        return GetPaymentsUseCase.MapToDto(payment);
    }
}
