using Lamour.Application.Features.Categories.Dtos;
using Lamour.Application.Features.Categories.Repositories;
using Lamour.Domain.Entities;
using Microsoft.Extensions.Logging;

namespace Lamour.Application.Features.Categories.UseCases;

public class GetCategoriesUseCase : IGetCategoriesUseCase
{
    private readonly ICategoryRepository _repo;
    private readonly ILogger<GetCategoriesUseCase> _logger;

    public GetCategoriesUseCase(ICategoryRepository repo, ILogger<GetCategoriesUseCase> logger)
    {
        _repo   = repo;
        _logger = logger;
    }

    public async Task<IEnumerable<CategoryResponseDto>> ExecuteAsync(CancellationToken ct = default)
    {
        _logger.LogInformation("Fetching all categories");
        var categories = await _repo.GetAllAsync(ct);
        return categories.Select(MapToDto);
    }

    internal static CategoryResponseDto MapToDto(Category c) => new()
    {
        Id   = c.Id,
        Name = c.Name,
    };
}
