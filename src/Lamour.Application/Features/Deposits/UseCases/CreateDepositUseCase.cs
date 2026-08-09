using Lamour.Application.Features.Deposits.Dtos;
using Lamour.Application.Features.Deposits.Repositories;
using Lamour.Domain.Entities;
using Lamour.Domain.Exceptions;
using Microsoft.Extensions.Logging;

namespace Lamour.Application.Features.Deposits.UseCases;

public class CreateDepositUseCase : ICreateDepositUseCase
{
    private readonly IDepositRepository _repo;
    private readonly ILogger<CreateDepositUseCase> _logger;

    public CreateDepositUseCase(IDepositRepository repo, ILogger<CreateDepositUseCase> logger)
    {
        _repo   = repo;
        _logger = logger;
    }

    public async Task<DepositResponseDto> ExecuteAsync(CreateDepositRequestDto request, CancellationToken ct = default)
    {
        if (request.Amount <= 0)
            throw new DomainException("Số tiền cọc phải lớn hơn 0.");

        var deposit = new Deposit
        {
            DocumentNumber   = request.DocumentNumber,
            AccountingDate   = DateTime.SpecifyKind(request.AccountingDate, DateTimeKind.Utc),
            DocumentDate     = DateTime.SpecifyKind(request.DocumentDate,   DateTimeKind.Utc),
            CustomerId       = request.CustomerId,
            EmployeeId       = request.EmployeeId,
            Description      = request.Description,
            Reference        = request.Reference,
            Amount           = request.Amount,
            RemainingBalance = request.Amount,
            Status           = DepositStatus.Active,
            CreatedAt        = DateTime.UtcNow,
        };

        var saved = await _repo.AddAsync(deposit, ct);

        _logger.LogInformation("Created Deposit {DocumentNumber} for customer {CustomerId}",
            saved.DocumentNumber, saved.CustomerId);

        return GetDepositsUseCase.MapToDto(saved);
    }
}
