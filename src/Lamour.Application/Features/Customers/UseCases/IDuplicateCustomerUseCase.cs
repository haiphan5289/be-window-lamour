using Lamour.Application.Features.Customers.Dtos;

namespace Lamour.Application.Features.Customers.UseCases;

public interface IDuplicateCustomerUseCase
{
    Task<CustomerResponseDto> ExecuteAsync(int id, CancellationToken ct = default);
}
