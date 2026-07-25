using Lamour.Application.Features.Categories.Dtos;

namespace Lamour.Application.Features.Categories.UseCases;

public interface ICreateCategoryUseCase
{
    Task<CategoryResponseDto> ExecuteAsync(CreateCategoryRequestDto request, CancellationToken ct = default);
}
