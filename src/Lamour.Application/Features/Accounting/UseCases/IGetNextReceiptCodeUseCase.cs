namespace Lamour.Application.Features.Accounting.UseCases;

public interface IGetNextReceiptCodeUseCase
{
    Task<string> ExecuteAsync(CancellationToken ct = default);
}
