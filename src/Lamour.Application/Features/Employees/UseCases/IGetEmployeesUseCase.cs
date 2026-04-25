using Lamour.Application.Features.Employees.Dtos;

namespace Lamour.Application.Features.Employees.UseCases;

public interface IGetEmployeesUseCase
{
    Task<IEnumerable<EmployeeResponseDto>> ExecuteAsync(CancellationToken ct = default);
}
