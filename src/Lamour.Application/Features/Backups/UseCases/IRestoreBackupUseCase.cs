namespace Lamour.Application.Features.Backups.UseCases;

public interface IRestoreBackupUseCase
{
    Task ExecuteAsync(string fileName, string password, int currentEmployeeId, CancellationToken ct = default);
}
