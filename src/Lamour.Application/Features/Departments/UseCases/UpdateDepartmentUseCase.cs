using Lamour.Application.Abstractions;
using Lamour.Application.Features.Departments.Dtos;
using Lamour.Application.Features.Departments.Repositories;
using Lamour.Domain.Exceptions;
using Microsoft.Extensions.Logging;

namespace Lamour.Application.Features.Departments.UseCases;

public class UpdateDepartmentUseCase : IUpdateDepartmentUseCase
{
    private readonly IDepartmentRepository _repo;
    private readonly INotificationBroadcaster _broadcaster;
    private readonly ILogger<UpdateDepartmentUseCase> _logger;

    public UpdateDepartmentUseCase(IDepartmentRepository repo, INotificationBroadcaster broadcaster, ILogger<UpdateDepartmentUseCase> logger)
    {
        _repo        = repo;
        _broadcaster = broadcaster;
        _logger      = logger;
    }

    public async Task<DepartmentResponseDto> ExecuteAsync(int id, UpdateDepartmentRequestDto request, CancellationToken ct = default)
    {
        var department = await _repo.GetByIdAsync(id, ct)
            ?? throw new NotFoundException($"Department {id} not found.");

        if (string.IsNullOrWhiteSpace(request.Name))
            throw new DomainException("Tên phòng ban không được để trống.");

        var name = request.Name.Trim();
        if (await _repo.NameExistsAsync(name, excludeId: id, ct: ct))
            throw new DomainException($"Phòng ban '{name}' đã tồn tại.");

        department.Name = name;
        var updated = await _repo.UpdateAsync(department, ct);
        _logger.LogInformation("Updated department {Id}", id);

        var dto = GetDepartmentsUseCase.MapToDto(updated);
        await _broadcaster.DepartmentUpdatedAsync(dto, ct);
        return dto;
    }
}
