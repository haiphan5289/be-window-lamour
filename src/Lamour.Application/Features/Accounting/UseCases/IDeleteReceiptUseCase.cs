namespace Lamour.Application.Features.Accounting.UseCases;

public interface IDeleteReceiptUseCase
{
    Task ExecuteAsync(int id, CancellationToken ct = default);
}
