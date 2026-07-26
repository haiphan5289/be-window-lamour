using Lamour.Domain.Entities;

namespace Lamour.Application.Features.Backups.Repositories;

public interface IBackupScheduleRepository
{
    Task<BackupSchedule> GetAsync(CancellationToken ct = default);
    Task<BackupSchedule> UpdateAsync(BackupSchedule schedule, CancellationToken ct = default);
}
