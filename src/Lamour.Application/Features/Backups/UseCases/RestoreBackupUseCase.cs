using Lamour.Application.Features.Backups.Repositories;
using Lamour.Application.Features.Employees.Repositories;
using Lamour.Application.Features.Employees.UseCases;
using Lamour.Domain.Exceptions;
using Microsoft.Extensions.Logging;

namespace Lamour.Application.Features.Backups.UseCases;

public class RestoreBackupUseCase : IRestoreBackupUseCase
{
    private readonly IBackupRepository          _backupRepo;
    private readonly IEmployeeRepository        _employeeRepo;
    private readonly ILogger<RestoreBackupUseCase> _logger;

    public RestoreBackupUseCase(
        IBackupRepository             backupRepo,
        IEmployeeRepository           employeeRepo,
        ILogger<RestoreBackupUseCase> logger)
    {
        _backupRepo   = backupRepo;
        _employeeRepo = employeeRepo;
        _logger       = logger;
    }

    public async Task ExecuteAsync(string fileName, string password, int currentEmployeeId, CancellationToken ct = default)
    {
        var employee = await _employeeRepo.GetByIdAsync(currentEmployeeId, ct)
            ?? throw new NotFoundException("Employee not found.");

        if (CreateEmployeeUseCase.HashPassword(password) != employee.PasswordHash)
            throw new DomainException("Mật khẩu không đúng.");

        _logger.LogWarning("Employee {Id} ({Name}) is restoring database from backup {File}.", employee.Id, employee.Name, fileName);

        await _backupRepo.RestoreAsync(fileName, ct);
    }
}
