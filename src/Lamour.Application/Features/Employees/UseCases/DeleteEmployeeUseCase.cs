using Lamour.Application.Abstractions;
using Lamour.Application.Features.Employees.Repositories;
using Lamour.Domain.Exceptions;
using Microsoft.Extensions.Logging;

namespace Lamour.Application.Features.Employees.UseCases;

public class DeleteEmployeeUseCase : IDeleteEmployeeUseCase
{
    private readonly IEmployeeRepository _repo;
    private readonly INotificationBroadcaster _broadcaster;
    private readonly ILogger<DeleteEmployeeUseCase> _logger;

    public DeleteEmployeeUseCase(IEmployeeRepository repo, INotificationBroadcaster broadcaster, ILogger<DeleteEmployeeUseCase> logger)
    {
        _repo        = repo;
        _broadcaster = broadcaster;
        _logger      = logger;
    }

    public async Task ExecuteAsync(int id, CancellationToken ct = default)
    {
        var employee = await _repo.GetByIdAsync(id, ct)
            ?? throw new NotFoundException($"Nhân viên {id} không tồn tại.");

        await _repo.DeleteAsync(employee, ct);
        _logger.LogInformation("Deleted employee {Id}", id);

        await _broadcaster.EmployeeDeletedAsync(id, ct);
    }
}
