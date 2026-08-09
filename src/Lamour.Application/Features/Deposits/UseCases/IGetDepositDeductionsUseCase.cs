using Lamour.Application.Features.Deposits.Dtos;

namespace Lamour.Application.Features.Deposits.UseCases;

public interface IGetDepositDeductionsUseCase
{
    Task<IEnumerable<DepositDeductionResponseDto>> ExecuteAsync(
        int? customerId, int? employeeId, int? salesOrderId,
        DateTime? fromDate, DateTime? toDate, CancellationToken ct = default);
}
