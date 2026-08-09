using Lamour.Application.Features.ProductUnits.Dtos;

namespace Lamour.Application.Features.ProductUnits.UseCases;

public interface ICreateProductUnitUseCase
{
    Task<ProductUnitResponseDto> ExecuteAsync(CreateProductUnitRequestDto request, CancellationToken ct = default);
}
