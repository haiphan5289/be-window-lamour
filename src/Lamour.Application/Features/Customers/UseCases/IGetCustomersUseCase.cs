using Lamour.Application.Features.Customers.Dtos;

namespace Lamour.Application.Features.Customers.UseCases;

public interface IGetCustomersUseCase
{
    Task<IEnumerable<CustomerResponseDto>> ExecuteAsync(CancellationToken ct = default);
}
