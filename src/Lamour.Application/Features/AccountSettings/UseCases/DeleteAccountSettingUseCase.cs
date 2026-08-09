using Lamour.Application.Abstractions;
using Lamour.Application.Features.AccountSettings.Repositories;
using Lamour.Domain.Exceptions;
using Microsoft.Extensions.Logging;

namespace Lamour.Application.Features.AccountSettings.UseCases;

public class DeleteAccountSettingUseCase : IDeleteAccountSettingUseCase
{
    private readonly IAccountSettingRepository _repo;
    private readonly INotificationBroadcaster _broadcaster;
    private readonly ILogger<DeleteAccountSettingUseCase> _logger;

    public DeleteAccountSettingUseCase(IAccountSettingRepository repo, INotificationBroadcaster broadcaster, ILogger<DeleteAccountSettingUseCase> logger)
    {
        _repo        = repo;
        _broadcaster = broadcaster;
        _logger      = logger;
    }

    public async Task ExecuteAsync(int id, CancellationToken ct = default)
    {
        var account = await _repo.GetByIdAsync(id, ct)
            ?? throw new NotFoundException($"Account setting {id} not found.");

        await _repo.DeleteAsync(account, ct);
        _logger.LogInformation("Deleted account setting {Id}", id);

        await _broadcaster.AccountSettingDeletedAsync(id, ct);
    }
}
