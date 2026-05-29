using Lamour.Application.Features.Customers.Dtos;

namespace Lamour.Application.Features.Customers.UseCases;

public interface IImportExcelCustomersUseCase
{
    Task<ImportCustomerResultDto> ExecuteAsync(Stream excelStream, CancellationToken ct = default);
}
