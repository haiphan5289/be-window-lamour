using Lamour.Application.Features.AccountSettings.Dtos;

namespace Lamour.Application.Features.AccountSettings.UseCases;

public interface IGetAccountSettingsUseCase
{
    Task<IEnumerable<AccountSettingResponseDto>> ExecuteAsync(CancellationToken ct = default);
}
