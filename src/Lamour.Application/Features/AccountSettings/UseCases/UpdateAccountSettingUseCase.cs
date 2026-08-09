using Lamour.Application.Abstractions;
using Lamour.Application.Features.AccountSettings.Dtos;
using Lamour.Application.Features.AccountSettings.Repositories;
using Lamour.Domain.Exceptions;
using Microsoft.Extensions.Logging;

namespace Lamour.Application.Features.AccountSettings.UseCases;

public class UpdateAccountSettingUseCase : IUpdateAccountSettingUseCase
{
    private readonly IAccountSettingRepository _repo;
    private readonly INotificationBroadcaster _broadcaster;
    private readonly ILogger<UpdateAccountSettingUseCase> _logger;

    public UpdateAccountSettingUseCase(IAccountSettingRepository repo, INotificationBroadcaster broadcaster, ILogger<UpdateAccountSettingUseCase> logger)
    {
        _repo        = repo;
        _broadcaster = broadcaster;
        _logger      = logger;
    }

    public async Task<AccountSettingResponseDto> ExecuteAsync(int id, UpdateAccountSettingRequestDto request, CancellationToken ct = default)
    {
        var account = await _repo.GetByIdAsync(id, ct)
            ?? throw new NotFoundException($"Account setting {id} not found.");

        if (string.IsNullOrWhiteSpace(request.Code))
            throw new DomainException("Số tài khoản không được để trống.");
        if (string.IsNullOrWhiteSpace(request.Description))
            throw new DomainException("Tên tài khoản không được để trống.");

        var code = request.Code.Trim();
        if (await _repo.CodeExistsAsync(code, excludeId: id, ct: ct))
            throw new DomainException($"Tài khoản '{code}' đã tồn tại.");

        account.Code        = code;
        account.Description = request.Description.Trim();
        var updated = await _repo.UpdateAsync(account, ct);
        _logger.LogInformation("Updated account setting {Id}", id);

        var dto = GetAccountSettingsUseCase.MapToDto(updated);
        await _broadcaster.AccountSettingUpdatedAsync(dto, ct);
        return dto;
    }
}
