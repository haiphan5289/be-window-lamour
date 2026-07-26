using Lamour.Application.Features.Backups.Dtos;

namespace Lamour.Application.Features.Backups.UseCases;

public interface IUpdateBackupScheduleUseCase
{
    Task<BackupScheduleResponseDto> ExecuteAsync(UpdateBackupScheduleRequestDto request, CancellationToken ct = default);
}
