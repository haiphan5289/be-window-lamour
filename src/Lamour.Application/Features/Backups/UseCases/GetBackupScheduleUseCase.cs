using Lamour.Application.Features.Backups.Dtos;
using Lamour.Application.Features.Backups.Repositories;
using Lamour.Domain.Entities;

namespace Lamour.Application.Features.Backups.UseCases;

public class GetBackupScheduleUseCase : IGetBackupScheduleUseCase
{
    private readonly IBackupScheduleRepository _repo;
    public GetBackupScheduleUseCase(IBackupScheduleRepository repo) => _repo = repo;

    public async Task<BackupScheduleResponseDto> ExecuteAsync(CancellationToken ct = default)
    {
        var schedule = await _repo.GetAsync(ct);
        return MapToDto(schedule);
    }

    internal static BackupScheduleResponseDto MapToDto(BackupSchedule s) => new()
    {
        IsEnabled     = s.IsEnabled,
        TimeOfDay     = s.TimeOfDay.ToString("HH:mm"),
        IntervalDays  = s.IntervalDays,
        RetentionDays = s.RetentionDays,
        Directory     = s.Directory,
        LastRunAt     = s.LastRunAt,
    };
}
