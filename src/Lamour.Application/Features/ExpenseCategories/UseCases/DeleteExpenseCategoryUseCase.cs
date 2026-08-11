using Lamour.Application.Abstractions;
using Lamour.Application.Features.ExpenseCategories.Repositories;
using Lamour.Domain.Exceptions;
using Microsoft.Extensions.Logging;

namespace Lamour.Application.Features.ExpenseCategories.UseCases;

public class DeleteExpenseCategoryUseCase : IDeleteExpenseCategoryUseCase
{
    private readonly IExpenseCategoryRepository _repo;
    private readonly INotificationBroadcaster _broadcaster;
    private readonly ILogger<DeleteExpenseCategoryUseCase> _logger;

    public DeleteExpenseCategoryUseCase(IExpenseCategoryRepository repo, INotificationBroadcaster broadcaster, ILogger<DeleteExpenseCategoryUseCase> logger)
    {
        _repo        = repo;
        _broadcaster = broadcaster;
        _logger      = logger;
    }

    public async Task ExecuteAsync(int id, CancellationToken ct = default)
    {
        var category = await _repo.GetByIdAsync(id, ct)
            ?? throw new NotFoundException($"Expense category {id} not found.");

        await _repo.DeleteAsync(category, ct);
        _logger.LogInformation("Deleted expense category {Id}", id);

        await _broadcaster.ExpenseCategoryDeletedAsync(id, ct);
    }
}
