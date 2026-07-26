using System.Diagnostics;
using Lamour.Application.Features.Backups.Repositories;
using Lamour.Domain.Exceptions;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Npgsql;

namespace Lamour.Infrastructure.Repositories;

public class BackupRepository : IBackupRepository
{
    private readonly string                        _defaultDirectory;
    private readonly string                        _pgDumpPath;
    private readonly string                        _psqlPath;
    private readonly NpgsqlConnectionStringBuilder  _dbConnection;
    private readonly IBackupScheduleRepository      _scheduleRepo;
    private readonly ILogger<BackupRepository>      _logger;

    public BackupRepository(IConfiguration config, IBackupScheduleRepository scheduleRepo, ILogger<BackupRepository> logger)
    {
        _defaultDirectory = config["BackupSettings:Directory"]
            ?? throw new InvalidOperationException("BackupSettings:Directory chưa được cấu hình.");
        _pgDumpPath   = config["BackupSettings:PgDumpPath"] ?? "pg_dump";
        _psqlPath     = config["BackupSettings:PsqlPath"] ?? "psql";
        _dbConnection = new NpgsqlConnectionStringBuilder(config.GetConnectionString("DefaultConnection"));
        _scheduleRepo = scheduleRepo;
        _logger       = logger;
    }

    private async Task<string> ResolveDirectoryAsync(CancellationToken ct)
    {
        var schedule  = await _scheduleRepo.GetAsync(ct);
        var directory = string.IsNullOrWhiteSpace(schedule.Directory) ? _defaultDirectory : schedule.Directory;
        System.IO.Directory.CreateDirectory(directory);
        return directory;
    }

    public async Task<IEnumerable<BackupFileInfo>> GetAllAsync(CancellationToken ct = default)
    {
        var directory = await ResolveDirectoryAsync(ct);
        return new DirectoryInfo(directory)
            .GetFiles("lamour_backup_*.sql")
            .OrderByDescending(f => f.CreationTimeUtc)
            .Select(f => new BackupFileInfo(f.Name, f.Length, f.CreationTimeUtc));
    }

    public async Task<BackupFileInfo> CreateAsync(CancellationToken ct = default)
    {
        var directory = await ResolveDirectoryAsync(ct);
        var fileName  = $"lamour_backup_{DateTime.UtcNow:yyyyMMdd_HHmmss}.sql";
        var fullPath  = Path.Combine(directory, fileName);

        var psi = new ProcessStartInfo
        {
            FileName               = _pgDumpPath,
            RedirectStandardError  = true,
            RedirectStandardOutput = false,
            UseShellExecute        = false,
        };
        psi.ArgumentList.Add("--no-owner");
        psi.ArgumentList.Add("--no-privileges");
        psi.ArgumentList.Add("--clean");
        psi.ArgumentList.Add("--if-exists");
        psi.ArgumentList.Add("-h"); psi.ArgumentList.Add(_dbConnection.Host ?? "localhost");
        psi.ArgumentList.Add("-p"); psi.ArgumentList.Add((_dbConnection.Port > 0 ? _dbConnection.Port : 5432).ToString());
        psi.ArgumentList.Add("-U"); psi.ArgumentList.Add(_dbConnection.Username ?? string.Empty);
        psi.ArgumentList.Add("-d"); psi.ArgumentList.Add(_dbConnection.Database ?? string.Empty);
        psi.ArgumentList.Add("-f"); psi.ArgumentList.Add(fullPath);

        if (!string.IsNullOrEmpty(_dbConnection.Password))
            psi.Environment["PGPASSWORD"] = _dbConnection.Password;

        _logger.LogInformation("Starting pg_dump backup to {File}", fullPath);

        using var process = Process.Start(psi)
            ?? throw new DomainException("Không thể khởi chạy pg_dump.");

        var stderr = await process.StandardError.ReadToEndAsync(ct);
        await process.WaitForExitAsync(ct);

        if (process.ExitCode != 0)
        {
            if (File.Exists(fullPath)) File.Delete(fullPath);
            _logger.LogError("pg_dump failed with exit code {Code}: {Error}", process.ExitCode, stderr);
            throw new DomainException($"Tạo bản sao lưu thất bại: {stderr}");
        }

        var info = new FileInfo(fullPath);
        _logger.LogInformation("Backup created: {File} ({Size} bytes)", fileName, info.Length);
        return new BackupFileInfo(fileName, info.Length, info.CreationTimeUtc);
    }

    public async Task<bool> DeleteAsync(string fileName, CancellationToken ct = default)
    {
        var directory = await ResolveDirectoryAsync(ct);
        var safeName  = Path.GetFileName(fileName);
        var fullPath  = Path.Combine(directory, safeName);

        if (!File.Exists(fullPath))
            return false;

        File.Delete(fullPath);
        _logger.LogInformation("Deleted backup {File}", safeName);
        return true;
    }

    public async Task RestoreAsync(string fileName, CancellationToken ct = default)
    {
        var directory = await ResolveDirectoryAsync(ct);
        var safeName  = Path.GetFileName(fileName);
        var fullPath  = Path.Combine(directory, safeName);

        if (!File.Exists(fullPath))
            throw new NotFoundException($"Backup file '{safeName}' not found.");

        var psi = new ProcessStartInfo
        {
            FileName               = _psqlPath,
            RedirectStandardError  = true,
            RedirectStandardOutput = false,
            UseShellExecute        = false,
        };
        psi.ArgumentList.Add("-v"); psi.ArgumentList.Add("ON_ERROR_STOP=1");
        psi.ArgumentList.Add("-h"); psi.ArgumentList.Add(_dbConnection.Host ?? "localhost");
        psi.ArgumentList.Add("-p"); psi.ArgumentList.Add((_dbConnection.Port > 0 ? _dbConnection.Port : 5432).ToString());
        psi.ArgumentList.Add("-U"); psi.ArgumentList.Add(_dbConnection.Username ?? string.Empty);
        psi.ArgumentList.Add("-d"); psi.ArgumentList.Add(_dbConnection.Database ?? string.Empty);
        psi.ArgumentList.Add("-f"); psi.ArgumentList.Add(fullPath);

        if (!string.IsNullOrEmpty(_dbConnection.Password))
            psi.Environment["PGPASSWORD"] = _dbConnection.Password;

        _logger.LogWarning("Restoring database from backup {File} — existing data will be dropped.", safeName);

        using var process = Process.Start(psi)
            ?? throw new DomainException("Không thể khởi chạy psql.");

        var stderr = await process.StandardError.ReadToEndAsync(ct);
        await process.WaitForExitAsync(ct);

        // Existing pooled connections may hold query plans referencing objects
        // dropped/recreated by the restore; force new connections to pick up
        // the fresh schema instead of erroring on stale cached plans.
        NpgsqlConnection.ClearAllPools();

        if (process.ExitCode != 0)
        {
            _logger.LogError("psql restore failed with exit code {Code}: {Error}", process.ExitCode, stderr);
            throw new DomainException($"Phục hồi thất bại: {stderr}");
        }

        _logger.LogWarning("Database restored successfully from {File}.", safeName);
    }

    public async Task<int> DeleteOlderThanAsync(int retentionDays, CancellationToken ct = default)
    {
        var directory = await ResolveDirectoryAsync(ct);
        var cutoffUtc = DateTime.UtcNow.AddDays(-retentionDays);
        var oldFiles  = new DirectoryInfo(directory)
            .GetFiles("lamour_backup_*.sql")
            .Where(f => f.CreationTimeUtc < cutoffUtc)
            .ToList();

        foreach (var file in oldFiles)
            file.Delete();

        if (oldFiles.Count > 0)
            _logger.LogInformation("Retention cleanup: deleted {Count} backup(s) older than {Days} days", oldFiles.Count, retentionDays);

        return oldFiles.Count;
    }
}
