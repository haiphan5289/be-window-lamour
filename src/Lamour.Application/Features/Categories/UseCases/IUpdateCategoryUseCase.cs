using Lamour.Application.Features.Categories.Dtos;

namespace Lamour.Application.Features.Categories.UseCases;

public interface IUpdateCategoryUseCase
{
    Task<CategoryResponseDto> ExecuteAsync(int id, UpdateCategoryRequestDto request, CancellationToken ct = default);
}
