using Lamour.Application.Features.Products.Dtos;

namespace Lamour.Application.Features.Products.UseCases;

public interface IDuplicateProductUseCase
{
    Task<ProductResponseDto> ExecuteAsync(int id, CancellationToken ct = default);
}
