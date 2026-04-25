using Lamour.Application.Features.Customers.Dtos;

namespace Lamour.Application.Features.Customers.UseCases;

public interface IUpdateCustomerUseCase
{
    Task<CustomerResponseDto> ExecuteAsync(int id, UpdateCustomerRequestDto request, CancellationToken ct = default);
}
