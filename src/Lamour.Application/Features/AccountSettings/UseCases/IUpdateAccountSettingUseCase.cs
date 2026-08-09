using Lamour.Application.Features.AccountSettings.Dtos;

namespace Lamour.Application.Features.AccountSettings.UseCases;

public interface IUpdateAccountSettingUseCase
{
    Task<AccountSettingResponseDto> ExecuteAsync(int id, UpdateAccountSettingRequestDto request, CancellationToken ct = default);
}
