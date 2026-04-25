using Lamour.Application.Features.Employees.Dtos;

namespace Lamour.Application.Features.Employees.UseCases;

public interface IDuplicateEmployeeUseCase
{
    Task<EmployeeResponseDto> ExecuteAsync(int id, CancellationToken ct = default);
}
