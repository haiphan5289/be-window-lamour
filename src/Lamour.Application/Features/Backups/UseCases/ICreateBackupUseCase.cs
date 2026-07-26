using Lamour.Application.Features.Backups.Dtos;

namespace Lamour.Application.Features.Backups.UseCases;

public interface ICreateBackupUseCase
{
    Task<BackupResponseDto> ExecuteAsync(CancellationToken ct = default);
}
