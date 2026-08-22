using Lamour.Application.Features.Accounting.Dtos;

namespace Lamour.Application.Features.Accounting.UseCases;

public interface ICreateBulkCustomerReceiptUseCase
{
    Task<CreateBulkCustomerReceiptResponseDto> ExecuteAsync(
        CreateBulkCustomerReceiptRequestDto request, CancellationToken ct = default);
}
