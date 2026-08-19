using Lamour.Application.Features.Employees.Dtos;
using Lamour.Application.Features.Employees.UseCases;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Lamour.Api.Controllers;

[ApiController]
[Route("api/v1/employees")]
[Authorize]
public class EmployeesController : ControllerBase
{
    private readonly IGetEmployeesUseCase      _getAll;
    private readonly ICreateEmployeeUseCase    _create;
    private readonly IUpdateEmployeeUseCase    _update;
    private readonly IDeleteEmployeeUseCase    _delete;
    private readonly IDuplicateEmployeeUseCase _duplicate;
    private readonly IImportExcelEmployeesUseCase _importExcel;

    public EmployeesController(
        IGetEmployeesUseCase      getAll,
        ICreateEmployeeUseCase    create,
        IUpdateEmployeeUseCase    update,
        IDeleteEmployeeUseCase    delete,
        IDuplicateEmployeeUseCase duplicate,
        IImportExcelEmployeesUseCase importExcel)
    {
        _getAll      = getAll;
        _create      = create;
        _update      = update;
        _delete      = delete;
        _duplicate   = duplicate;
        _importExcel = importExcel;
    }

    [HttpGet]
    public async Task<IActionResult> GetAll(CancellationToken ct)
    {
        var result = await _getAll.ExecuteAsync(ct);
        return Ok(result);
    }

    [HttpPost]
    public async Task<IActionResult> Create([FromBody] CreateEmployeeRequestDto request, CancellationToken ct)
    {
        var result = await _create.ExecuteAsync(request, ct);
        return CreatedAtAction(nameof(GetAll), new { }, result);
    }

    [HttpPut("{id:int}")]
    public async Task<IActionResult> Update(int id, [FromBody] UpdateEmployeeRequestDto request, CancellationToken ct)
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

    [HttpPost("{id:int}/duplicate")]
    public async Task<IActionResult> Duplicate(int id, CancellationToken ct)
    {
        var result = await _duplicate.ExecuteAsync(id, ct);
        return CreatedAtAction(nameof(GetAll), new { }, result);
    }

    [HttpPost("import-excel")]
    public async Task<IActionResult> ImportExcel(IFormFile file, CancellationToken ct)
    {
        if (file is null || file.Length == 0)
            return BadRequest(new { message = "File không được để trống." });

        using var stream = file.OpenReadStream();
        var result = await _importExcel.ExecuteAsync(stream, ct);
        return Ok(result);
    }
}
