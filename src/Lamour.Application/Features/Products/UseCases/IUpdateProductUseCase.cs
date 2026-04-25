using Lamour.Application.Features.Products.Dtos;

namespace Lamour.Application.Features.Products.UseCases;

public interface IUpdateProductUseCase
{
    Task<ProductResponseDto> ExecuteAsync(int id, UpdateProductRequestDto request, CancellationToken ct = default);
}
