using Lamour.Application.Features.Backups.Dtos;
using Lamour.Application.Features.Backups.Repositories;

namespace Lamour.Application.Features.Backups.UseCases;

public class GetBackupsUseCase : IGetBackupsUseCase
{
    private readonly IBackupRepository _repo;
    public GetBackupsUseCase(IBackupRepository repo) => _repo = repo;

    public async Task<IEnumerable<BackupResponseDto>> ExecuteAsync(CancellationToken ct = default)
    {
        var files = await _repo.GetAllAsync(ct);
        return files.Select(MapToDto);
    }

    internal static BackupResponseDto MapToDto(BackupFileInfo f) => new()
    {
        FileName  = f.FileName,
        SizeBytes = f.SizeBytes,
        CreatedAt = f.CreatedAt,
    };
}
