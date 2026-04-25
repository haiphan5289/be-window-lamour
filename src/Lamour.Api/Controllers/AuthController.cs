using Lamour.Application.Features.Auth.Dtos;
using Lamour.Application.Features.Auth.UseCases;
using Microsoft.AspNetCore.Mvc;

namespace Lamour.Api.Controllers;

[ApiController]
[Route("api/v1/auth")]
public class AuthController : ControllerBase
{
    private readonly ILoginUseCase _loginUseCase;

    public AuthController(ILoginUseCase loginUseCase) => _loginUseCase = loginUseCase;

    [HttpPost("login")]
    public async Task<IActionResult> Login([FromBody] LoginRequestDto request, CancellationToken ct)
    {
        var result = await _loginUseCase.ExecuteAsync(request, ct);
        return Ok(result);
    }
}
