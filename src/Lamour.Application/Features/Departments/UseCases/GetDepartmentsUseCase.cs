using Lamour.Application.Features.Departments.Dtos;
using Lamour.Application.Features.Departments.Repositories;
using Lamour.Domain.Entities;
using Microsoft.Extensions.Logging;

namespace Lamour.Application.Features.Departments.UseCases;

public class GetDepartmentsUseCase : IGetDepartmentsUseCase
{
    private readonly IDepartmentRepository _repo;
    private readonly ILogger<GetDepartmentsUseCase> _logger;

    public GetDepartmentsUseCase(IDepartmentRepository repo, ILogger<GetDepartmentsUseCase> logger)
    {
        _repo   = repo;
        _logger = logger;
    }

    public async Task<IEnumerable<DepartmentResponseDto>> ExecuteAsync(CancellationToken ct = default)
    {
        _logger.LogInformation("Fetching all departments");
        var departments = await _repo.GetAllAsync(ct);
        return departments.Select(MapToDto);
    }

    internal static DepartmentResponseDto MapToDto(Department d) => new()
    {
        Id   = d.Id,
        Name = d.Name,
    };
}
