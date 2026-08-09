namespace Lamour.Application.Features.AccountSettings.UseCases;

public interface IDeleteAccountSettingUseCase
{
    Task ExecuteAsync(int id, CancellationToken ct = default);
}
