using Lamour.Application.Features.Suppliers.Dtos;

namespace Lamour.Application.Features.Suppliers.UseCases;

public interface IGetSuppliersUseCase
{
    Task<IEnumerable<SupplierResponseDto>> ExecuteAsync(CancellationToken ct = default);
}
