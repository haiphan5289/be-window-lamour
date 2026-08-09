using Lamour.Application.Features.AccountSettings.Dtos;
using Lamour.Application.Features.AccountSettings.UseCases;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Lamour.Api.Controllers;

[ApiController]
[Route("api/v1/account-settings")]
[Authorize]
public class AccountSettingsController : ControllerBase
{
    private readonly IGetAccountSettingsUseCase   _getAll;
    private readonly ICreateAccountSettingUseCase _create;
    private readonly IUpdateAccountSettingUseCase _update;
    private readonly IDeleteAccountSettingUseCase _delete;

    public AccountSettingsController(
        IGetAccountSettingsUseCase   getAll,
        ICreateAccountSettingUseCase create,
        IUpdateAccountSettingUseCase update,
        IDeleteAccountSettingUseCase delete)
    {
        _getAll = getAll;
        _create = create;
        _update = update;
        _delete = delete;
    }

    [HttpGet]
    public async Task<IActionResult> GetAll(CancellationToken ct)
    {
        var result = await _getAll.ExecuteAsync(ct);
        return Ok(result);
    }

    [HttpPost]
    public async Task<IActionResult> Create([FromBody] CreateAccountSettingRequestDto request, CancellationToken ct)
    {
        var result = await _create.ExecuteAsync(request, ct);
        return CreatedAtAction(nameof(GetAll), new { }, result);
    }

    [HttpPut("{id:int}")]
    public async Task<IActionResult> Update(int id, [FromBody] UpdateAccountSettingRequestDto request, CancellationToken ct)
    {
        var result = await _update.ExecuteAsync(id, request, ct);
        return Ok(result);
    }

    [HttpDelete("{id:int}")]
    public async Task<IActionResult> Delete(int id, CancellationToken ct)
    {
        await _delete.ExecuteAsync(id, ct);
        return NoContent();
    }
}
