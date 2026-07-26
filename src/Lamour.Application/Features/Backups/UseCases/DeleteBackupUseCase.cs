using Lamour.Application.Features.Backups.Repositories;
using Lamour.Domain.Exceptions;

namespace Lamour.Application.Features.Backups.UseCases;

public class DeleteBackupUseCase : IDeleteBackupUseCase
{
    private readonly IBackupRepository _repo;
    public DeleteBackupUseCase(IBackupRepository repo) => _repo = repo;

    public async Task ExecuteAsync(string fileName, CancellationToken ct = default)
    {
        var deleted = await _repo.DeleteAsync(fileName, ct);
        if (!deleted)
            throw new NotFoundException($"Backup file '{fileName}' not found.");
    }
}
