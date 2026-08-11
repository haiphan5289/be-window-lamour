using Lamour.Application.Features.Accounting.Repositories;
using Lamour.Domain.Entities;
using Lamour.Domain.Exceptions;
using Microsoft.Extensions.Logging;

namespace Lamour.Application.Features.Accounting.UseCases;

public class DeletePaymentUseCase : IDeletePaymentUseCase
{
    private readonly IPaymentRepository     _repo;
    private readonly ILogger<DeletePaymentUseCase> _logger;

    public DeletePaymentUseCase(
        IPaymentRepository repo,
        ILogger<DeletePaymentUseCase> logger)
    {
        _repo     = repo;
        _logger   = logger;
    }

    public async Task ExecuteAsync(int id, CancellationToken ct = default)
    {
        var payment = await _repo.GetByIdAsync(id, ct)
            ?? throw new NotFoundException($"Payment with id {id} not found.");

        if (payment.Status == PaymentStatus.Confirmed)
            throw new DomainException("Phiếu chi đã ghi số, không thể xoá.");

        await _repo.DeleteAsync(payment, ct);

        _logger.LogInformation("Deleted Payment {Id} ({DocumentNumber})", id, payment.DocumentNumber);
    }
}
