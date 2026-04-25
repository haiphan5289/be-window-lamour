using Lamour.Application.Features.Products.Dtos;

namespace Lamour.Application.Features.Products.UseCases;

public interface IGetProductsUseCase
{
    Task<IEnumerable<ProductResponseDto>> ExecuteAsync(CancellationToken ct = default);
}
