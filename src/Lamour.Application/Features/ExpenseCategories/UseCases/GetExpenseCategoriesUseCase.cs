using Lamour.Application.Features.ExpenseCategories.Dtos;
using Lamour.Application.Features.ExpenseCategories.Repositories;
using Lamour.Domain.Entities;
using Microsoft.Extensions.Logging;

namespace Lamour.Application.Features.ExpenseCategories.UseCases;

public class GetExpenseCategoriesUseCase : IGetExpenseCategoriesUseCase
{
    private readonly IExpenseCategoryRepository _repo;
    private readonly ILogger<GetExpenseCategoriesUseCase> _logger;

    public GetExpenseCategoriesUseCase(IExpenseCategoryRepository repo, ILogger<GetExpenseCategoriesUseCase> logger)
    {
        _repo   = repo;
        _logger = logger;
    }

    public async Task<IEnumerable<ExpenseCategoryResponseDto>> ExecuteAsync(CancellationToken ct = default)
    {
        _logger.LogInformation("Fetching all expense categories");
        var categories = await _repo.GetAllAsync(ct);
        return categories.Select(MapToDto);
    }

    internal static ExpenseCategoryResponseDto MapToDto(ExpenseCategory e) => new()
    {
        Id             = e.Id,
        Code           = e.Code,
        Name           = e.Name,
        DepartmentId   = e.DepartmentId,
        DepartmentName = e.Department?.Name,
        Description    = e.Description,
    };
}
