using Lamour.Application.Features.Backups.Dtos;
using Lamour.Application.Features.Backups.Repositories;
using Microsoft.Extensions.Logging;

namespace Lamour.Application.Features.Backups.UseCases;

public class CreateBackupUseCase : ICreateBackupUseCase
{
    private readonly IBackupRepository        _repo;
    private readonly ILogger<CreateBackupUseCase> _logger;

    public CreateBackupUseCase(IBackupRepository repo, ILogger<CreateBackupUseCase> logger)
    {
        _repo   = repo;
        _logger = logger;
    }

    public async Task<BackupResponseDto> ExecuteAsync(CancellationToken ct = default)
    {
        var created = await _repo.CreateAsync(ct);
        _logger.LogInformation("Backup {File} created.", created.FileName);
        return GetBackupsUseCase.MapToDto(created);
    }
}
