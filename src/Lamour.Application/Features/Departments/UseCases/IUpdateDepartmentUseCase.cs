using Lamour.Application.Features.Departments.Dtos;

namespace Lamour.Application.Features.Departments.UseCases;

public interface IUpdateDepartmentUseCase
{
    Task<DepartmentResponseDto> ExecuteAsync(int id, UpdateDepartmentRequestDto request, CancellationToken ct = default);
}
