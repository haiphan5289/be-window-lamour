using Lamour.Application.Features.Employees.Dtos;

namespace Lamour.Application.Features.Employees.UseCases;

public interface IUpdateEmployeeUseCase
{
    Task<EmployeeResponseDto> ExecuteAsync(int id, UpdateEmployeeRequestDto request, CancellationToken ct = default);
}
