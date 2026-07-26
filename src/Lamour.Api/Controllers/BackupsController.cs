using Lamour.Application.Features.Backups.Dtos;
using Lamour.Application.Features.Backups.UseCases;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace Lamour.Api.Controllers;

[ApiController]
[Route("api/v1/backups")]
[Authorize(Roles = "Admin")]
public class BackupsController : ControllerBase
{
    private readonly IGetBackupsUseCase    _getAll;
    private readonly ICreateBackupUseCase  _create;
    private readonly IDeleteBackupUseCase  _delete;
    private readonly IRestoreBackupUseCase _restore;

    public BackupsController(
        IGetBackupsUseCase    getAll,
        ICreateBackupUseCase  create,
        IDeleteBackupUseCase  delete,
        IRestoreBackupUseCase restore)
    {
        _getAll  = getAll;
        _create  = create;
        _delete  = delete;
        _restore = restore;
    }

    [HttpGet]
    public async Task<IActionResult> GetAll(CancellationToken ct)
    {
        var result = await _getAll.ExecuteAsync(ct);
        return Ok(result);
    }

    [HttpPost]
    public async Task<IActionResult> Create(CancellationToken ct)
    {
        var result = await _create.ExecuteAsync(ct);
        return CreatedAtAction(nameof(GetAll), new { }, result);
    }

    [HttpDelete("{fileName}")]
    public async Task<IActionResult> Delete(string fileName, CancellationToken ct)
    {
        await _delete.ExecuteAsync(fileName, ct);
        return NoContent();
    }

    [HttpPost("{fileName}/restore")]
    public async Task<IActionResult> Restore(string fileName, [FromBody] RestoreBackupRequestDto request, CancellationToken ct)
    {
        var employeeId = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);
        await _restore.ExecuteAsync(fileName, request.Password, employeeId, ct);
        return NoContent();
    }
}
