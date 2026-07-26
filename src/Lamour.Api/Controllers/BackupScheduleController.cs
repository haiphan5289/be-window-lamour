using Lamour.Application.Features.Backups.Dtos;
using Lamour.Application.Features.Backups.UseCases;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Lamour.Api.Controllers;

[ApiController]
[Route("api/v1/backup-schedule")]
[Authorize(Roles = "Admin")]
public class BackupScheduleController : ControllerBase
{
    private readonly IGetBackupScheduleUseCase    _getSchedule;
    private readonly IUpdateBackupScheduleUseCase _updateSchedule;

    public BackupScheduleController(
        IGetBackupScheduleUseCase    getSchedule,
        IUpdateBackupScheduleUseCase updateSchedule)
    {
        _getSchedule    = getSchedule;
        _updateSchedule = updateSchedule;
    }

    [HttpGet]
    public async Task<IActionResult> Get(CancellationToken ct)
    {
        var result = await _getSchedule.ExecuteAsync(ct);
        return Ok(result);
    }

    [HttpPut]
    public async Task<IActionResult> Update([FromBody] UpdateBackupScheduleRequestDto request, CancellationToken ct)
    {
        var result = await _updateSchedule.ExecuteAsync(request, ct);
        return Ok(result);
    }
}
