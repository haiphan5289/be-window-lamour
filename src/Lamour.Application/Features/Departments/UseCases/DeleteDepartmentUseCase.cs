using Lamour.Application.Abstractions;
using Lamour.Application.Features.Departments.Repositories;
using Lamour.Domain.Exceptions;
using Microsoft.Extensions.Logging;

namespace Lamour.Application.Features.Departments.UseCases;

public class DeleteDepartmentUseCase : IDeleteDepartmentUseCase
{
    private readonly IDepartmentRepository _repo;
    private readonly INotificationBroadcaster _broadcaster;
    private readonly ILogger<DeleteDepartmentUseCase> _logger;

    public DeleteDepartmentUseCase(IDepartmentRepository repo, INotificationBroadcaster broadcaster, ILogger<DeleteDepartmentUseCase> logger)
    {
        _repo        = repo;
        _broadcaster = broadcaster;
        _logger      = logger;
    }

    public async Task ExecuteAsync(int id, CancellationToken ct = default)
    {
        var department = await _repo.GetByIdAsync(id, ct)
            ?? throw new NotFoundException($"Department {id} not found.");

        if (await _repo.IsInUseAsync(id, ct))
            throw new DomainException($"Phòng ban '{department.Name}' đang được khoản mục chi phí sử dụng, không thể xoá.");

        await _repo.DeleteAsync(department, ct);
        _logger.LogInformation("Deleted department {Id}", id);

        await _broadcaster.DepartmentDeletedAsync(id, ct);
    }
}
