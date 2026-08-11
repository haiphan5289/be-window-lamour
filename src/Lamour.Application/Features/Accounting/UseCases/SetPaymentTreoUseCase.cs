using Lamour.Application.Features.Accounting.Dtos;
using Lamour.Application.Features.Accounting.Repositories;
using Lamour.Domain.Entities;
using Lamour.Domain.Exceptions;
using Microsoft.Extensions.Logging;

namespace Lamour.Application.Features.Accounting.UseCases;

public class SetPaymentTreoUseCase : ISetPaymentTreoUseCase
{
    private readonly IPaymentRepository _repo;
    private readonly ILogger<SetPaymentTreoUseCase> _logger;

    public SetPaymentTreoUseCase(IPaymentRepository repo, ILogger<SetPaymentTreoUseCase> logger)
    {
        _repo   = repo;
        _logger = logger;
    }

    public async Task<PaymentResponseDto> ExecuteAsync(int id, CancellationToken ct = default)
    {
        var payment = await _repo.GetByIdTrackedAsync(id, ct)
            ?? throw new NotFoundException($"Payment with id {id} not found.");

        if (payment.Status != PaymentStatus.Draft)
            throw new DomainException("Chỉ phiếu chi ở trạng thái Nháp mới có thể chuyển Treo.");

        payment.Status = PaymentStatus.Treo;
        await _repo.UpdateAsync(payment, ct);

        _logger.LogInformation("Set Payment {Id} ({DocumentNumber}) to Treo", id, payment.DocumentNumber);

        return GetPaymentsUseCase.MapToDto(payment);
    }
}
