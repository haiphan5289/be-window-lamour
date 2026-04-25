using Lamour.Application.Features.Customers.Dtos;

namespace Lamour.Application.Features.Customers.UseCases;

public interface ICreateCustomerUseCase
{
    Task<CustomerResponseDto> ExecuteAsync(CreateCustomerRequestDto request, CancellationToken ct = default);
}
