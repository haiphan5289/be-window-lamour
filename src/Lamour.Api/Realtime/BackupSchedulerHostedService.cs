using Lamour.Application.Features.Backups.Repositories;
using Lamour.Application.Features.Backups.UseCases;

namespace Lamour.Api.Realtime;

/// <summary>
/// Checks every minute whether the configured backup schedule (every N days,
/// at a fixed time of day) should fire. Runs entirely server-side so scheduled
/// backups happen regardless of whether any WPF client is open.
/// </summary>
public class BackupSchedulerHostedService : BackgroundService
{
    private static readonly TimeSpan CheckInterval = TimeSpan.FromMinutes(1);

    private readonly IServiceScopeFactory                  _scopeFactory;
    private readonly ILogger<BackupSchedulerHostedService> _logger;

    public BackupSchedulerHostedService(IServiceScopeFactory scopeFactory, ILogger<BackupSchedulerHostedService> logger)
    {
        _scopeFactory = scopeFactory;
        _logger       = logger;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        while (!stoppingToken.IsCancellationRequested)
        {
            try
            {
                await CheckAndRunAsync(stoppingToken);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Backup scheduler tick failed.");
            }

            await Task.Delay(CheckInterval, stoppingToken);
        }
    }

    private async Task CheckAndRunAsync(CancellationToken ct)
    {
        using var scope = _scopeFactory.CreateScope();
        var scheduleRepo = scope.ServiceProvider.GetRequiredService<IBackupScheduleRepository>();
        var schedule     = await scheduleRepo.GetAsync(ct);

        if (!schedule.IsEnabled) return;

        var now = DateTime.Now;
        var lastRunLocal = schedule.LastRunAt is { } lastRunUtc
            ? DateTime.SpecifyKind(lastRunUtc, DateTimeKind.Utc).ToLocalTime()
            : (DateTime?)null;

        var nextDueDate     = lastRunLocal?.Date.AddDays(schedule.IntervalDays) ?? now.Date;
        var isDue           = now.Date >= nextDueDate;
        var triggerStart    = schedule.TimeOfDay.ToTimeSpan();
        var triggerEnd      = triggerStart.Add(CheckInterval);
        var isTriggerWindow = now.TimeOfDay >= triggerStart && now.TimeOfDay < triggerEnd;

        if (!isDue || !isTriggerWindow) return;

        _logger.LogInformation("Scheduled backup triggered at {Time}", now);

        var createBackup = scope.ServiceProvider.GetRequiredService<ICreateBackupUseCase>();
        await createBackup.ExecuteAsync(ct);

        var backupRepo = scope.ServiceProvider.GetRequiredService<IBackupRepository>();
        var deleted    = await backupRepo.DeleteOlderThanAsync(schedule.RetentionDays, ct);
        if (deleted > 0)
            _logger.LogInformation("Retention cleanup removed {Count} old backup(s)", deleted);

        schedule.LastRunAt = DateTime.UtcNow;
        await scheduleRepo.UpdateAsync(schedule, ct);
    }
}
