using Lamour.Application.Features.Suppliers.Dtos;

namespace Lamour.Application.Features.Suppliers.UseCases;

public interface IImportExcelSuppliersUseCase
{
    Task<ImportSupplierResultDto> ExecuteAsync(Stream excelStream, CancellationToken ct = default);
}
