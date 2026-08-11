using Lamour.Application.Features.Departments.Dtos;

namespace Lamour.Application.Features.Departments.UseCases;

public interface IGetDepartmentsUseCase
{
    Task<IEnumerable<DepartmentResponseDto>> ExecuteAsync(CancellationToken ct = default);
}
