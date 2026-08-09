using Lamour.Application.Features.AccountSettings.Dtos;
using Lamour.Application.Features.AccountSettings.Repositories;
using Lamour.Domain.Entities;
using Microsoft.Extensions.Logging;

namespace Lamour.Application.Features.AccountSettings.UseCases;

public class GetAccountSettingsUseCase : IGetAccountSettingsUseCase
{
    private readonly IAccountSettingRepository _repo;
    private readonly ILogger<GetAccountSettingsUseCase> _logger;

    public GetAccountSettingsUseCase(IAccountSettingRepository repo, ILogger<GetAccountSettingsUseCase> logger)
    {
        _repo   = repo;
        _logger = logger;
    }

    public async Task<IEnumerable<AccountSettingResponseDto>> ExecuteAsync(CancellationToken ct = default)
    {
        _logger.LogInformation("Fetching all account settings");
        var accounts = await _repo.GetAllAsync(ct);
        return accounts.Select(MapToDto);
    }

    internal static AccountSettingResponseDto MapToDto(AccountSetting a) => new()
    {
        Id          = a.Id,
        Code        = a.Code,
        Description = a.Description,
    };
}
