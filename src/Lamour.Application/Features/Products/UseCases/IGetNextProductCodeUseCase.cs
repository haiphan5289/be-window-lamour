namespace Lamour.Application.Features.Products.UseCases;

public interface IGetNextProductCodeUseCase
{
    Task<string> ExecuteAsync(CancellationToken ct = default);
}
