namespace Lamour.Application.Features.Departments.UseCases;

public interface IDeleteDepartmentUseCase
{
    Task ExecuteAsync(int id, CancellationToken ct = default);
}
