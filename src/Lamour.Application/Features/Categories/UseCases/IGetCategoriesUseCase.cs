using Lamour.Application.Features.Categories.Dtos;

namespace Lamour.Application.Features.Categories.UseCases;

public interface IGetCategoriesUseCase
{
    Task<IEnumerable<CategoryResponseDto>> ExecuteAsync(CancellationToken ct = default);
}
