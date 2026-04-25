using Lamour.Application.Features.Suppliers.Dtos;

namespace Lamour.Application.Features.Suppliers.UseCases;

public interface IDuplicateSupplierUseCase
{
    Task<SupplierResponseDto> ExecuteAsync(int id, CancellationToken ct = default);
}
