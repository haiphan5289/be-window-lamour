using Lamour.Application.Features.Backups.Dtos;

namespace Lamour.Application.Features.Backups.UseCases;

public interface IGetBackupsUseCase
{
    Task<IEnumerable<BackupResponseDto>> ExecuteAsync(CancellationToken ct = default);
}
