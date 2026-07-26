namespace Lamour.Application.Features.Backups.Repositories;

public record BackupFileInfo(string FileName, long SizeBytes, DateTime CreatedAt);

public interface IBackupRepository
{
    Task<IEnumerable<BackupFileInfo>> GetAllAsync(CancellationToken ct = default);
    Task<BackupFileInfo> CreateAsync(CancellationToken ct = default);
    Task<bool> DeleteAsync(string fileName, CancellationToken ct = default);
    Task<int> DeleteOlderThanAsync(int retentionDays, CancellationToken ct = default);
    Task RestoreAsync(string fileName, CancellationToken ct = default);
}
