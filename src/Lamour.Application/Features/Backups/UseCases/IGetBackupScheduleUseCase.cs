using Lamour.Application.Features.Backups.Dtos;

namespace Lamour.Application.Features.Backups.UseCases;

public interface IGetBackupScheduleUseCase
{
    Task<BackupScheduleResponseDto> ExecuteAsync(CancellationToken ct = default);
}
