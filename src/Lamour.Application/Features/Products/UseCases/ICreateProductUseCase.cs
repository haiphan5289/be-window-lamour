using Lamour.Application.Features.Products.Dtos;

namespace Lamour.Application.Features.Products.UseCases;

public interface ICreateProductUseCase
{
    Task<ProductResponseDto> ExecuteAsync(CreateProductRequestDto request, CancellationToken ct = default);
}
