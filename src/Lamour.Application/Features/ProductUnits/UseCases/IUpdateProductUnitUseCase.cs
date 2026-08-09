using Lamour.Application.Features.ProductUnits.Dtos;

namespace Lamour.Application.Features.ProductUnits.UseCases;

public interface IUpdateProductUnitUseCase
{
    Task<ProductUnitResponseDto> ExecuteAsync(int id, UpdateProductUnitRequestDto request, CancellationToken ct = default);
}
