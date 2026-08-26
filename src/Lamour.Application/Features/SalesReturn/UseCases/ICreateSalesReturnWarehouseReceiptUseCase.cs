using Lamour.Application.Features.WarehouseReceipts.Dtos;

namespace Lamour.Application.Features.SalesReturn.UseCases;

public interface ICreateSalesReturnWarehouseReceiptUseCase
{
    Task<WarehouseReceiptResponseDto> ExecuteAsync(int salesReturnId, CancellationToken ct = default);
}
