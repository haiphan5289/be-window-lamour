using Lamour.Application.Abstractions;
using Lamour.Application.Features.Departments.Repositories;
using Lamour.Application.Features.ExpenseCategories.Dtos;
using Lamour.Application.Features.ExpenseCategories.Repositories;
using Lamour.Domain.Entities;
using Lamour.Domain.Exceptions;
using Microsoft.Extensions.Logging;

namespace Lamour.Application.Features.ExpenseCategories.UseCases;

public class CreateExpenseCategoryUseCase : ICreateExpenseCategoryUseCase
{
    private readonly IExpenseCategoryRepository _repo;
    private readonly IDepartmentRepository _departmentRepo;
    private readonly INotificationBroadcaster _broadcaster;
    private readonly ILogger<CreateExpenseCategoryUseCase> _logger;

    public CreateExpenseCategoryUseCase(
        IExpenseCategoryRepository repo,
        IDepartmentRepository departmentRepo,
        INotificationBroadcaster broadcaster,
        ILogger<CreateExpenseCategoryUseCase> logger)
    {
        _repo           = repo;
        _departmentRepo = departmentRepo;
        _broadcaster    = broadcaster;
        _logger         = logger;
    }

    public async Task<ExpenseCategoryResponseDto> ExecuteAsync(CreateExpenseCategoryRequestDto request, CancellationToken ct = default)
    {
        if (string.IsNullOrWhiteSpace(request.Code))
            throw new DomainException("Mã khoản mục chi phí không được để trống.");
        if (string.IsNullOrWhiteSpace(request.Name))
            throw new DomainException("Tên khoản mục chi phí không được để trống.");

        var code = request.Code.Trim();
        var name = request.Name.Trim();

        if (await _repo.CodeExistsAsync(code, ct: ct))
            throw new DomainException($"Khoản mục chi phí '{code}' đã tồn tại.");

        if (request.DepartmentId is not null && await _departmentRepo.GetByIdAsync(request.DepartmentId.Value, ct) is null)
            throw new DomainException("Phòng ban không tồn tại.");

        var category = new ExpenseCategory
        {
            Code         = code,
            Name         = name,
            DepartmentId = request.DepartmentId,
            Description  = string.IsNullOrWhiteSpace(request.Description) ? null : request.Description.Trim(),
        };
        var created = await _repo.AddAsync(category, ct);
        _logger.LogInformation("Created expense category {Id} '{Code}'", created.Id, created.Code);

        var dto = GetExpenseCategoriesUseCase.MapToDto(created);
        await _broadcaster.ExpenseCategoryCreatedAsync(dto, ct);
        return dto;
    }
}
