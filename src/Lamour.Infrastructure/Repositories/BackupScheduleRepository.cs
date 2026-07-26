using Lamour.Application.Features.Backups.Repositories;
using Lamour.Domain.Entities;
using Lamour.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace Lamour.Infrastructure.Repositories;

public class BackupScheduleRepository : IBackupScheduleRepository
{
    private readonly AppDbContext _db;
    public BackupScheduleRepository(AppDbContext db) => _db = db;

    public async Task<BackupSchedule> GetAsync(CancellationToken ct = default)
        => await _db.BackupSchedules.AsNoTracking().FirstAsync(ct);

    public async Task<BackupSchedule> UpdateAsync(BackupSchedule schedule, CancellationToken ct = default)
    {
        _db.BackupSchedules.Update(schedule);
        await _db.SaveChangesAsync(ct);
        return schedule;
    }
}
