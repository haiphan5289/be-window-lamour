using Lamour.Application.Features.Departments.Dtos;

namespace Lamour.Application.Features.Departments.UseCases;

public interface ICreateDepartmentUseCase
{
    Task<DepartmentResponseDto> ExecuteAsync(CreateDepartmentRequestDto request, CancellationToken ct = default);
}
