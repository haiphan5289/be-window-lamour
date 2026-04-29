using Lamour.Application.Features.Accounting.Dtos;
using Lamour.Application.Features.Accounting.Repositories;
using Lamour.Domain.Exceptions;
using Microsoft.Extensions.Logging;

namespace Lamour.Application.Features.Accounting.UseCases;

public class GetPaymentByIdUseCase : IGetPaymentByIdUseCase
{
    private readonly IPaymentRepository _repo;
    private readonly ILogger<GetPaymentByIdUseCase> _logger;

    public GetPaymentByIdUseCase(IPaymentRepository repo, ILogger<GetPaymentByIdUseCase> logger)
    {
        _repo   = repo;
        _logger = logger;
    }

    public async Task<PaymentResponseDto> ExecuteAsync(int id, CancellationToken ct = default)
    {
        _logger.LogInformation("Fetching payment {Id}", id);
        var payment = await _repo.GetByIdAsync(id, ct)
            ?? throw new NotFoundException($"Payment with id {id} not found.");
        return GetPaymentsUseCase.MapToDto(payment);
    }
}
