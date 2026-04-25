using Lamour.Application.Features.Employees.Dtos;

namespace Lamour.Application.Features.Employees.UseCases;

public interface ICreateEmployeeUseCase
{
    Task<EmployeeResponseDto> ExecuteAsync(CreateEmployeeRequestDto request, CancellationToken ct = default);
}
