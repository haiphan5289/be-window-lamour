using Lamour.Application.Features.Products.Dtos;

namespace Lamour.Application.Features.Products.UseCases;

public interface IImportExcelProductsUseCase
{
    Task<ImportProductResultDto> ExecuteAsync(Stream excelStream, CancellationToken ct = default);
}
