using Lamour.Application.Abstractions;
using Lamour.Application.Features.Deposits.Dtos;
using Lamour.Application.Features.Deposits.Repositories;
using Lamour.Application.Features.Sales.Repositories;
using Lamour.Domain.Entities;
using Lamour.Domain.Exceptions;
using Microsoft.Extensions.Logging;

namespace Lamour.Application.Features.Deposits.UseCases;

public class CreateDepositDeductionUseCase : ICreateDepositDeductionUseCase
{
    private readonly IDepositRepository          _depositRepo;
    private readonly IDepositDeductionRepository _deductionRepo;
    private readonly ISalesOrderRepository       _salesOrderRepo;
    private readonly IUnitOfWork                 _uow;
    private readonly ILogger<CreateDepositDeductionUseCase> _logger;

    public CreateDepositDeductionUseCase(
        IDepositRepository depositRepo,
        IDepositDeductionRepository deductionRepo,
        ISalesOrderRepository salesOrderRepo,
        IUnitOfWork uow,
        ILogger<CreateDepositDeductionUseCase> logger)
    {
        _depositRepo    = depositRepo;
        _deductionRepo  = deductionRepo;
        _salesOrderRepo = salesOrderRepo;
        _uow            = uow;
        _logger         = logger;
    }

    public async Task<DepositDeductionResponseDto> ExecuteAsync(
        CreateDepositDeductionRequestDto request, CancellationToken ct = default)
    {
        var deposit = await _depositRepo.GetByIdTrackedAsync(request.DepositId, ct)
            ?? throw new NotFoundException($"Deposit {request.DepositId} not found.");

        var salesOrder = await _salesOrderRepo.GetByIdAsync(request.SalesOrderId, ct)
            ?? throw new NotFoundException($"Sales order {request.SalesOrderId} not found.");

        if (request.Amount <= 0)
            throw new DomainException("Số tiền trừ cọc phải lớn hơn 0.");

        if (request.Amount > deposit.RemainingBalance)
            throw new DomainException("Số tiền trừ cọc vượt quá số dư còn lại.");

        await _uow.BeginAsync(ct);
        try
        {
            var nextNum = await _deductionRepo.GetNextCodeNumberAsync(ct);

            var deduction = new DepositDeduction
            {
                DocumentNumber = $"TC{nextNum:D5}",
                DepositId      = deposit.Id,
                SalesOrderId   = salesOrder.Id,
                Amount         = request.Amount,
                AccountingDate = DateTime.SpecifyKind(request.AccountingDate, DateTimeKind.Utc),
                DocumentDate   = DateTime.SpecifyKind(request.DocumentDate,   DateTimeKind.Utc),
                Description    = request.Description,
                CreatedAt      = DateTime.UtcNow,
            };

            var saved = await _deductionRepo.AddAsync(deduction, ct);

            deposit.RemainingBalance -= request.Amount;
            deposit.Status = deposit.RemainingBalance == 0 ? DepositStatus.Depleted : DepositStatus.Active;
            await _depositRepo.UpdateAsync(deposit, ct);

            await _uow.CommitAsync(ct);

            _logger.LogInformation("Created DepositDeduction {DocumentNumber} for deposit {DepositId}, sales order {SalesOrderId}",
                saved.DocumentNumber, deposit.Id, salesOrder.Id);

            return GetDepositDeductionsUseCase.MapToDto(saved);
        }
        catch
        {
            await _uow.RollbackAsync(ct);
            throw;
        }
    }
}
