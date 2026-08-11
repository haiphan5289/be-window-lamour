using Lamour.Application.Abstractions;
using Lamour.Application.Features.Departments.Repositories;
using Lamour.Application.Features.ExpenseCategories.Dtos;
using Lamour.Application.Features.ExpenseCategories.Repositories;
using Lamour.Domain.Exceptions;
using Microsoft.Extensions.Logging;

namespace Lamour.Application.Features.ExpenseCategories.UseCases;

public class UpdateExpenseCategoryUseCase : IUpdateExpenseCategoryUseCase
{
    private readonly IExpenseCategoryRepository _repo;
    private readonly IDepartmentRepository _departmentRepo;
    private readonly INotificationBroadcaster _broadcaster;
    private readonly ILogger<UpdateExpenseCategoryUseCase> _logger;

    public UpdateExpenseCategoryUseCase(
        IExpenseCategoryRepository repo,
        IDepartmentRepository departmentRepo,
        INotificationBroadcaster broadcaster,
        ILogger<UpdateExpenseCategoryUseCase> logger)
    {
        _repo           = repo;
        _departmentRepo = departmentRepo;
        _broadcaster    = broadcaster;
        _logger         = logger;
    }

    public async Task<ExpenseCategoryResponseDto> ExecuteAsync(int id, UpdateExpenseCategoryRequestDto request, CancellationToken ct = default)
    {
        var category = await _repo.GetByIdAsync(id, ct)
            ?? throw new NotFoundException($"Expense category {id} not found.");

        if (string.IsNullOrWhiteSpace(request.Code))
            throw new DomainException("Mã khoản mục chi phí không được để trống.");
        if (string.IsNullOrWhiteSpace(request.Name))
            throw new DomainException("Tên khoản mục chi phí không được để trống.");

        var code = request.Code.Trim();
        var name = request.Name.Trim();

        if (await _repo.CodeExistsAsync(code, excludeId: id, ct: ct))
            throw new DomainException($"Khoản mục chi phí '{code}' đã tồn tại.");

        if (request.DepartmentId is not null && await _departmentRepo.GetByIdAsync(request.DepartmentId.Value, ct) is null)
            throw new DomainException("Phòng ban không tồn tại.");

        category.Code         = code;
        category.Name         = name;
        category.DepartmentId = request.DepartmentId;
        category.Description  = string.IsNullOrWhiteSpace(request.Description) ? null : request.Description.Trim();

        var updated = await _repo.UpdateAsync(category, ct);
        _logger.LogInformation("Updated expense category {Id}", id);

        var dto = GetExpenseCategoriesUseCase.MapToDto(updated);
        await _broadcaster.ExpenseCategoryUpdatedAsync(dto, ct);
        return dto;
    }
}
