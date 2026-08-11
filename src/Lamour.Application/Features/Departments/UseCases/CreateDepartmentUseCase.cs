using Lamour.Application.Abstractions;
using Lamour.Application.Features.Departments.Dtos;
using Lamour.Application.Features.Departments.Repositories;
using Lamour.Domain.Entities;
using Lamour.Domain.Exceptions;
using Microsoft.Extensions.Logging;

namespace Lamour.Application.Features.Departments.UseCases;

public class CreateDepartmentUseCase : ICreateDepartmentUseCase
{
    private readonly IDepartmentRepository _repo;
    private readonly INotificationBroadcaster _broadcaster;
    private readonly ILogger<CreateDepartmentUseCase> _logger;

    public CreateDepartmentUseCase(IDepartmentRepository repo, INotificationBroadcaster broadcaster, ILogger<CreateDepartmentUseCase> logger)
    {
        _repo        = repo;
        _broadcaster = broadcaster;
        _logger      = logger;
    }

    public async Task<DepartmentResponseDto> ExecuteAsync(CreateDepartmentRequestDto request, CancellationToken ct = default)
    {
        if (string.IsNullOrWhiteSpace(request.Name))
            throw new DomainException("Tên phòng ban không được để trống.");

        var name = request.Name.Trim();
        if (await _repo.NameExistsAsync(name, ct: ct))
            throw new DomainException($"Phòng ban '{name}' đã tồn tại.");

        var department = new Department { Name = name };
        var created    = await _repo.AddAsync(department, ct);
        _logger.LogInformation("Created department {Id} '{Name}'", created.Id, created.Name);

        var dto = GetDepartmentsUseCase.MapToDto(created);
        await _broadcaster.DepartmentCreatedAsync(dto, ct);
        return dto;
    }
}
