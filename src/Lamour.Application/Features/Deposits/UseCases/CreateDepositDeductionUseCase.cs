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

    public async Task<IEnumerable<DepositDeductionResponseDto>> ExecuteAsync(
        CreateDepositDeductionRequestDto request, CancellationToken ct = default)
    {
        var salesOrder = await _salesOrderRepo.GetByIdAsync(request.SalesOrderId, ct)
            ?? throw new NotFoundException($"Sales order {request.SalesOrderId} not found.");

        if (request.Amount <= 0)
            throw new DomainException("Số tiền trừ cọc phải lớn hơn 0.");

        var eligibleDeposits = (await _depositRepo.GetEligibleForDeductionAsync(
            salesOrder.CustomerId, request.SalesOrderId, ct)).ToList();

        var totalAvailable = eligibleDeposits.Sum(d => d.RemainingBalance);
        if (request.Amount > totalAvailable)
            throw new DomainException("Số tiền trừ cọc vượt quá tổng số dư cọc còn lại của khách hàng.");

        await _uow.BeginAsync(ct);
        try
        {
            var results = new List<DepositDeductionResponseDto>();
            var remainingToAllocate = request.Amount;

            foreach (var deposit in eligibleDeposits)
            {
                if (remainingToAllocate <= 0)
                    break;

                var slice = Math.Min(remainingToAllocate, deposit.RemainingBalance);

                var nextNum = await _deductionRepo.GetNextCodeNumberAsync(ct);

                var deduction = new DepositDeduction
                {
                    DocumentNumber = $"TC{nextNum:D5}",
                    DepositId      = deposit.Id,
                    SalesOrderId   = salesOrder.Id,
                    Amount         = slice,
                    AccountingDate = DateTime.SpecifyKind(request.AccountingDate, DateTimeKind.Utc),
                    DocumentDate   = DateTime.SpecifyKind(request.DocumentDate,   DateTimeKind.Utc),
                    Description    = request.Description,
                    CreatedAt      = DateTime.UtcNow,
                };

                var saved = await _deductionRepo.AddAsync(deduction, ct);

                deposit.RemainingBalance -= slice;
                deposit.Status = deposit.RemainingBalance == 0 ? DepositStatus.Depleted : DepositStatus.Active;
                await _depositRepo.UpdateAsync(deposit, ct);

                results.Add(GetDepositDeductionsUseCase.MapToDto(saved));
                remainingToAllocate -= slice;
            }

            await _uow.CommitAsync(ct);

            _logger.LogInformation("Created {Count} DepositDeduction rows totaling {Amount} for sales order {SalesOrderId}",
                results.Count, request.Amount, salesOrder.Id);

            return results;
        }
        catch
        {
            await _uow.RollbackAsync(ct);
            throw;
        }
    }
}
