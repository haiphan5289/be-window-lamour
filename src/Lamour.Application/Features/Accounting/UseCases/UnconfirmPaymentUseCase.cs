using Lamour.Application.Features.Accounting.Dtos;
using Lamour.Application.Features.Accounting.Repositories;
using Lamour.Domain.Entities;
using Lamour.Domain.Exceptions;
using Microsoft.Extensions.Logging;

namespace Lamour.Application.Features.Accounting.UseCases;

// "Hoàn" (MISA) — đưa phiếu chi đã Ghi số quay lại Treo, xoá CashTransaction đã tạo lúc Confirm.
// Mirror UnconfirmWarehouseReceiptUseCase (WarehouseReceipts feature).
public class UnconfirmPaymentUseCase : IUnconfirmPaymentUseCase
{
    private readonly IPaymentRepository    _repo;
    private readonly ICashLedgerRepository _cashRepo;
    private readonly ILogger<UnconfirmPaymentUseCase> _logger;

    public UnconfirmPaymentUseCase(
        IPaymentRepository repo,
        ICashLedgerRepository cashRepo,
        ILogger<UnconfirmPaymentUseCase> logger)
    {
        _repo     = repo;
        _cashRepo = cashRepo;
        _logger   = logger;
    }

    public async Task<PaymentResponseDto> ExecuteAsync(int id, CancellationToken ct = default)
    {
        var payment = await _repo.GetByIdTrackedAsync(id, ct)
            ?? throw new NotFoundException($"Payment with id {id} not found.");

        if (payment.Status != PaymentStatus.Confirmed)
            throw new DomainException("Chỉ phiếu chi đã Ghi số mới có thể Hoàn.");

        await _cashRepo.DeleteByPaymentNumberAsync(payment.DocumentNumber, ct);

        payment.Status      = PaymentStatus.Treo;
        payment.ConfirmedAt = null;
        await _repo.UpdateAsync(payment, ct);

        _logger.LogInformation("Unconfirmed Payment {Id} ({DocumentNumber}) — reverted to Treo", id, payment.DocumentNumber);

        return GetPaymentsUseCase.MapToDto(payment);
    }
}
