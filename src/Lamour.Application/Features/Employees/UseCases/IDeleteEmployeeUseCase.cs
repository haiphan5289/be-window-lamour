namespace Lamour.Application.Features.Employees.UseCases;

public interface IDeleteEmployeeUseCase
{
    Task ExecuteAsync(int id, CancellationToken ct = default);
}
