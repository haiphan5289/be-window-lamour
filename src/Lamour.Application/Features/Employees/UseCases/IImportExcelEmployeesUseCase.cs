using Lamour.Application.Features.Employees.Dtos;

namespace Lamour.Application.Features.Employees.UseCases;

public interface IImportExcelEmployeesUseCase
{
    Task<ImportEmployeeResultDto> ExecuteAsync(Stream excelStream, CancellationToken ct = default);
}
