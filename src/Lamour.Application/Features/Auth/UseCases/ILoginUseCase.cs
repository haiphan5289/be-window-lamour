using Lamour.Application.Features.Auth.Dtos;

namespace Lamour.Application.Features.Auth.UseCases;

public interface ILoginUseCase
{
    Task<LoginResponseDto> ExecuteAsync(LoginRequestDto request, CancellationToken ct = default);
}
