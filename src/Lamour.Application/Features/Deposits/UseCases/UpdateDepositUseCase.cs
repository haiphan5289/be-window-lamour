using Lamour.Application.Features.Deposits.Dtos;
using Lamour.Application.Features.Deposits.Repositories;
using Lamour.Domain.Exceptions;
using Microsoft.Extensions.Logging;

namespace Lamour.Application.Features.Deposits.UseCases;

public class UpdateDepositUseCase : IUpdateDepositUseCase
{
    private readonly IDepositRepository _repo;
    private readonly ILogger<UpdateDepositUseCase> _logger;

    public UpdateDepositUseCase(IDepositRepository repo, ILogger<UpdateDepositUseCase> logger)
    {
        _repo   = repo;
        _logger = logger;
    }

    public async Task<DepositResponseDto> ExecuteAsync(int id, UpdateDepositRequestDto request, CancellationToken ct = default)
    {
        var deposit = await _repo.GetByIdTrackedAsync(id, ct)
            ?? throw new NotFoundException($"Deposit {id} not found.");

        if (deposit.RemainingBalance != deposit.Amount)
            throw new DomainException("Cọc đã bị trừ, không thể sửa.");

        if (request.Amount <= 0)
            throw new DomainException("Số tiền cọc phải lớn hơn 0.");

        deposit.DocumentNumber   = request.DocumentNumber;
        deposit.AccountingDate   = DateTime.SpecifyKind(request.AccountingDate, DateTimeKind.Utc);
        deposit.DocumentDate     = DateTime.SpecifyKind(request.DocumentDate,   DateTimeKind.Utc);
        deposit.CustomerId       = request.CustomerId;
        deposit.EmployeeId       = request.EmployeeId;
        deposit.Description      = request.Description;
        deposit.Reference        = request.Reference;
        deposit.Amount           = request.Amount;
        deposit.RemainingBalance = request.Amount;

        await _repo.UpdateAsync(deposit, ct);

        _logger.LogInformation("Updated Deposit {Id}", id);

        var updated = await _repo.GetByIdAsync(id, ct);
        return GetDepositsUseCase.MapToDto(updated!);
    }
}
