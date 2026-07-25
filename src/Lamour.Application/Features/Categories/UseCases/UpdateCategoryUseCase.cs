using Lamour.Application.Abstractions;
using Lamour.Application.Features.Categories.Dtos;
using Lamour.Application.Features.Categories.Repositories;
using Lamour.Domain.Exceptions;
using Microsoft.Extensions.Logging;

namespace Lamour.Application.Features.Categories.UseCases;

public class UpdateCategoryUseCase : IUpdateCategoryUseCase
{
    private readonly ICategoryRepository _repo;
    private readonly INotificationBroadcaster _broadcaster;
    private readonly ILogger<UpdateCategoryUseCase> _logger;

    public UpdateCategoryUseCase(ICategoryRepository repo, INotificationBroadcaster broadcaster, ILogger<UpdateCategoryUseCase> logger)
    {
        _repo        = repo;
        _broadcaster = broadcaster;
        _logger      = logger;
    }

    public async Task<CategoryResponseDto> ExecuteAsync(int id, UpdateCategoryRequestDto request, CancellationToken ct = default)
    {
        var category = await _repo.GetByIdAsync(id, ct)
            ?? throw new NotFoundException($"Category {id} not found.");

        if (string.IsNullOrWhiteSpace(request.Name))
            throw new DomainException("Category name is required.");

        var name = request.Name.Trim();
        if (await _repo.NameExistsAsync(name, excludeId: id, ct: ct))
            throw new DomainException($"Category '{name}' already exists.");

        category.Name = name;
        var updated = await _repo.UpdateAsync(category, ct);
        _logger.LogInformation("Updated category {Id}", id);

        var dto = GetCategoriesUseCase.MapToDto(updated);
        await _broadcaster.CategoryUpdatedAsync(dto, ct);
        return dto;
    }
}
