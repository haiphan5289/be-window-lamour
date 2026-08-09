using Lamour.Application.Features.AccountSettings.Dtos;

namespace Lamour.Application.Features.AccountSettings.UseCases;

public interface ICreateAccountSettingUseCase
{
    Task<AccountSettingResponseDto> ExecuteAsync(CreateAccountSettingRequestDto request, CancellationToken ct = default);
}
