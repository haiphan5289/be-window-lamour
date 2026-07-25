using Lamour.Application.Abstractions;
using Lamour.Application.Features.Categories.Dtos;
using Lamour.Application.Features.Categories.Repositories;
using Lamour.Domain.Entities;
using Lamour.Domain.Exceptions;
using Microsoft.Extensions.Logging;

namespace Lamour.Application.Features.Categories.UseCases;

public class CreateCategoryUseCase : ICreateCategoryUseCase
{
    private readonly ICategoryRepository _repo;
    private readonly INotificationBroadcaster _broadcaster;
    private readonly ILogger<CreateCategoryUseCase> _logger;

    public CreateCategoryUseCase(ICategoryRepository repo, INotificationBroadcaster broadcaster, ILogger<CreateCategoryUseCase> logger)
    {
        _repo        = repo;
        _broadcaster = broadcaster;
        _logger      = logger;
    }

    public async Task<CategoryResponseDto> ExecuteAsync(CreateCategoryRequestDto request, CancellationToken ct = default)
    {
        if (string.IsNullOrWhiteSpace(request.Name))
            throw new DomainException("Category name is required.");

        var name = request.Name.Trim();
        if (await _repo.NameExistsAsync(name, ct: ct))
            throw new DomainException($"Category '{name}' already exists.");

        var category = new Category { Name = name };
        var created  = await _repo.AddAsync(category, ct);
        _logger.LogInformation("Created category {Id} '{Name}'", created.Id, created.Name);

        var dto = GetCategoriesUseCase.MapToDto(created);
        await _broadcaster.CategoryCreatedAsync(dto, ct);
        return dto;
    }
}
