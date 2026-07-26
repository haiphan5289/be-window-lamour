namespace Lamour.Application.Features.Backups.UseCases;

public interface IDeleteBackupUseCase
{
    Task ExecuteAsync(string fileName, CancellationToken ct = default);
}
