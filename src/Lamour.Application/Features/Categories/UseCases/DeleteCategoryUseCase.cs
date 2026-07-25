using Lamour.Application.Abstractions;
using Lamour.Application.Features.Categories.Repositories;
using Lamour.Domain.Exceptions;
using Microsoft.Extensions.Logging;

namespace Lamour.Application.Features.Categories.UseCases;

public class DeleteCategoryUseCase : IDeleteCategoryUseCase
{
    private readonly ICategoryRepository _repo;
    private readonly INotificationBroadcaster _broadcaster;
    private readonly ILogger<DeleteCategoryUseCase> _logger;

    public DeleteCategoryUseCase(ICategoryRepository repo, INotificationBroadcaster broadcaster, ILogger<DeleteCategoryUseCase> logger)
    {
        _repo        = repo;
        _broadcaster = broadcaster;
        _logger      = logger;
    }

    public async Task ExecuteAsync(int id, CancellationToken ct = default)
    {
        var category = await _repo.GetByIdAsync(id, ct)
            ?? throw new NotFoundException($"Category {id} not found.");

        if (await _repo.IsInUseAsync(id, ct))
            throw new DomainException($"Không thể xóa danh mục '{category.Name}' vì đang có sản phẩm sử dụng.");

        await _repo.DeleteAsync(category, ct);
        _logger.LogInformation("Deleted category {Id}", id);

        await _broadcaster.CategoryDeletedAsync(id, ct);
    }
}
