using Lamour.Application.Abstractions;
using Lamour.Application.Features.AccountSettings.Dtos;
using Lamour.Application.Features.AccountSettings.Repositories;
using Lamour.Domain.Entities;
using Lamour.Domain.Exceptions;
using Microsoft.Extensions.Logging;

namespace Lamour.Application.Features.AccountSettings.UseCases;

public class CreateAccountSettingUseCase : ICreateAccountSettingUseCase
{
    private readonly IAccountSettingRepository _repo;
    private readonly INotificationBroadcaster _broadcaster;
    private readonly ILogger<CreateAccountSettingUseCase> _logger;

    public CreateAccountSettingUseCase(IAccountSettingRepository repo, INotificationBroadcaster broadcaster, ILogger<CreateAccountSettingUseCase> logger)
    {
        _repo        = repo;
        _broadcaster = broadcaster;
        _logger      = logger;
    }

    public async Task<AccountSettingResponseDto> ExecuteAsync(CreateAccountSettingRequestDto request, CancellationToken ct = default)
    {
        if (string.IsNullOrWhiteSpace(request.Code))
            throw new DomainException("Số tài khoản không được để trống.");
        if (string.IsNullOrWhiteSpace(request.Description))
            throw new DomainException("Tên tài khoản không được để trống.");

        var code = request.Code.Trim();
        if (await _repo.CodeExistsAsync(code, ct: ct))
            throw new DomainException($"Tài khoản '{code}' đã tồn tại.");

        var account = new AccountSetting { Code = code, Description = request.Description.Trim() };
        var created = await _repo.AddAsync(account, ct);
        _logger.LogInformation("Created account setting {Id} '{Code}'", created.Id, created.Code);

        var dto = GetAccountSettingsUseCase.MapToDto(created);
        await _broadcaster.AccountSettingCreatedAsync(dto, ct);
        return dto;
    }
}
