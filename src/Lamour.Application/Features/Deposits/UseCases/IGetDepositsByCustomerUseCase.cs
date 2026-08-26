using Lamour.Application.Features.Deposits.Dtos;

namespace Lamour.Application.Features.Deposits.UseCases;

public interface IGetDepositsByCustomerUseCase
{
    Task<IEnumerable<DepositResponseDto>> ExecuteAsync(int customerId, int? excludeSalesOrderId = null, CancellationToken ct = default);
}
