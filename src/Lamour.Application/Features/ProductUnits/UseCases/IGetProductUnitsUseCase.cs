using Lamour.Application.Features.ProductUnits.Dtos;

namespace Lamour.Application.Features.ProductUnits.UseCases;

public interface IGetProductUnitsUseCase
{
    Task<IEnumerable<ProductUnitResponseDto>> ExecuteAsync(CancellationToken ct = default);
}
